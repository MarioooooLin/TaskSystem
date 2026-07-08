using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Kols.Commands;

public sealed class RejectKolHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo,
    ICurrentUser currentUser,
    IActivityLogRepository activityLogRepo)
{
    public async Task<Result> HandleAsync(RejectKolCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var kol = await kolRepo.GetByIdAsync(cmd.KolId, uow.Session, ct);
        if (kol is null)
            return Result.Failure(Errors.Kol.NotFound);

        // 只有 Pending 才可退回（重送審核後才會再次 Pending）
        if (kol.VerificationStatus != VerificationStatus.Pending)
            return Result.Failure(Errors.Kol.CannotApprove);

        kol.VerificationStatus = VerificationStatus.Rejected;
        kol.VerifiedAt = DateTime.UtcNow;
        kol.VerifiedByAdminId = currentUser.UserId;
        kol.RejectionNote = cmd.RejectionNote;
        kol.UpdatedAt = DateTime.UtcNow;

        await kolRepo.UpdateAsync(kol, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "KolProfiles",
            targetId: cmd.KolId,
            actorUserId: currentUser.UserId,
            action: "Reject",
            note: cmd.RejectionNote,
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
