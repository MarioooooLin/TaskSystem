using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Merchants.Commands;

public sealed class SuspendMerchantHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo,
    IUserRepository userRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(SuspendMerchantCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var merchant = await merchantRepo.GetByIdAsync(cmd.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Result.Failure(Errors.Merchant.NotFound);

        // 已是停用狀態，不重複操作
        if (merchant.VerificationStatus == VerificationStatus.Suspended)
            return Result.Failure(Errors.Merchant.AlreadySuspended);

        merchant.VerificationStatus = VerificationStatus.Suspended;
        merchant.VerifiedAt = DateTime.UtcNow;
        merchant.UpdatedByAdminId = currentUser.UserId;
        merchant.UpdatedAt = DateTime.UtcNow;

        await merchantRepo.UpdateAsync(merchant, uow.Session, ct);

        // 同步停用底下所有 Active 成員的登入帳號
        await userRepo.SuspendUsersByMerchantAsync(merchant.Id, uow.Session, ct);

        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
