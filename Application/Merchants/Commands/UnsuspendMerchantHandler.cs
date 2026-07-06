using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Merchants.Commands;

public sealed class UnsuspendMerchantHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(UnsuspendMerchantCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var merchant = await merchantRepo.GetByIdAsync(cmd.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Result.Failure(Errors.Merchant.NotFound);

        // 必須是停用狀態才能解除
        if (merchant.VerificationStatus != VerificationStatus.Suspended)
            return Result.Failure(Errors.Merchant.NotSuspended);

        merchant.VerificationStatus = VerificationStatus.Approved;
        merchant.VerifiedAt = DateTime.UtcNow;
        merchant.UpdatedByAdminId = currentUser.UserId;
        merchant.UpdatedAt = DateTime.UtcNow;

        await merchantRepo.UpdateAsync(merchant, uow.Session, ct);
        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
