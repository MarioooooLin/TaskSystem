using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Kols.Commands;

public sealed class ApproveKolHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(ApproveKolCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var kol = await kolRepo.GetByIdAsync(cmd.KolId, uow.Session, ct);
        if (kol is null)
            return Result.Failure(Errors.Kol.NotFound);

        // 只有 Pending 或 Rejected 才可審核通過
        if (kol.VerificationStatus != VerificationStatus.Pending &&
            kol.VerificationStatus != VerificationStatus.Rejected)
            return Result.Failure(Errors.Kol.CannotApprove);

        kol.VerificationStatus = VerificationStatus.Approved;
        kol.VerifiedAt = DateTime.UtcNow;
        kol.VerifiedByAdminId = currentUser.UserId;
        kol.RejectionNote = null;   // 通過後清除退回原因
        kol.UpdatedAt = DateTime.UtcNow;

        await kolRepo.UpdateAsync(kol, uow.Session, ct);
        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
