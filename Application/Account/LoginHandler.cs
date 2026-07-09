using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Account;

/// <summary>
/// Admin 登入 Use Case。
/// 驗證身份後回傳 LoginResult，Controller 用來建立 Cookie Claims。
/// </summary>
public sealed class LoginHandler(
    IUserRepository userRepo,
    IRoleRepository roleRepo,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand cmd,
        CancellationToken ct = default)
    {
        // 唯讀操作：BeginAsync 取得 Session，不需 Commit（auto-rollback 無害）
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 1. 以 Email 查詢帳號
        var user = await userRepo.GetByEmailAsync(cmd.Email, uow.Session, ct);
        if (user is null)
            return Errors.User.InvalidCredentials;

        // 2. 確認為 Admin 帳號
        if (user.AccountType != AccountType.Admin)
            return Errors.User.NotAdminAccount;

        // 3. 確認帳號狀態
        if (user.Status == UserStatus.Suspended)
            return Errors.User.AccountSuspended;

        if (user.Status == UserStatus.Deleted)
            return Errors.User.InvalidCredentials;

        // 4. 驗證密碼（PasswordHash 為 null 代表第三方帳號，不允許密碼登入）
        if (user.PasswordHash is null || !passwordHasher.Verify(cmd.Password, user.PasswordHash))
            return Errors.User.InvalidCredentials;

        // 5. 載入此帳號的 Permission 清單（UserRoles → RolePermissions → Permissions）
        var permissions = await roleRepo.GetPermissionCodesByUserIdAsync(user.Id, uow.Session, ct);

        return new LoginResult(
            UserId: user.Id,
            Name: user.Name,
            Email: user.Email,
            PermissionCodes: permissions
        );
    }
}
