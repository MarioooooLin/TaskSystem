using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Account;

/// <summary>
/// 業者端登入 Use Case。
/// 驗證統一編號、使用者身份與所屬組織後回傳 MerchantLoginResult，Controller 用來寫入 Cookie Claims。
/// </summary>
public sealed class MerchantLoginHandler(
    IUserRepository userRepo,
    IMerchantRepository merchantRepo,
    IMerchantMemberRepository memberRepo,
    IRoleRepository roleRepo,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<MerchantLoginResult>> HandleAsync(
        MerchantLoginCommand cmd,
        CancellationToken ct = default)
    {
        // 唯讀操作：BeginAsync 取得 Session，不需 Commit
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 1. 以統一編號查詢業者
        var merchant = await merchantRepo.GetByTaxIdAsync(cmd.TaxId, uow.Session, ct);
        if (merchant is null)
            return Errors.User.InvalidCredentials;

        // 2. 業者必須已通過審核
        if (merchant.VerificationStatus != VerificationStatus.Approved)
            return Errors.Merchant.NotApproved;

        // 3. 以電子郵件查詢帳號
        var user = await userRepo.GetByEmailAsync(cmd.Email, uow.Session, ct);
        if (user is null)
            return Errors.User.InvalidCredentials;

        // 4. 確認為業者帳號
        if (user.AccountType != AccountType.Merchant)
            return Errors.User.InvalidCredentials;

        // 5. 確認帳號狀態
        if (user.Status == UserStatus.Suspended)
            return Errors.User.AccountSuspended;

        if (user.Status == UserStatus.Deleted)
            return Errors.User.InvalidCredentials;

        // 6. 確認為該業者組織的 Active 成員
        var member = await memberRepo.GetByMerchantAndUserAsync(merchant.Id, user.Id, uow.Session, ct);
        if (member is null)
            return Errors.User.InvalidCredentials;

        if (member.Status != MerchantMemberStatus.Active)
            return Errors.Member.NotActive;

        // 7. 驗證密碼（PasswordHash 為 null 代表第三方帳號，不允許密碼登入）
        if (user.PasswordHash is null || !passwordHasher.Verify(cmd.Password, user.PasswordHash))
            return Errors.User.InvalidCredentials;

        // 8. 載入此帳號的 Permission 清單
        var permissions = await roleRepo.GetPermissionCodesByUserIdAsync(user.Id, uow.Session, ct);

        return new MerchantLoginResult(
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email,
            MerchantId: merchant.Id,
            CompanyName: merchant.CompanyName,
            PermissionCodes: permissions
        );
    }
}
