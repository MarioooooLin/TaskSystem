using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Kols.Commands;

public sealed class UnsuspendKolHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo,
    IUserRepository userRepo,
    ICurrentUser currentUser,
    IActivityLogRepository activityLogRepo)
{
    public async Task<Result> HandleAsync(UnsuspendKolCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var kol = await kolRepo.GetByIdAsync(cmd.KolId, uow.Session, ct);
        if (kol is null)
            return Result.Failure(Errors.Kol.NotFound);

        if (kol.VerificationStatus != VerificationStatus.Suspended)
            return Result.Failure(Errors.Kol.NotSuspended);

        kol.VerificationStatus = VerificationStatus.Approved;
        kol.VerifiedAt = DateTime.UtcNow;
        kol.VerifiedByAdminId = currentUser.UserId;
        kol.SuspensionNote = null;   // 解除後清除停權原因
        kol.UpdatedAt = DateTime.UtcNow;

        await kolRepo.UpdateAsync(kol, uow.Session, ct);

        // 同步恢復 KOL 登入帳號
        var user = await userRepo.GetByIdAsync(kol.UserId, uow.Session, ct);
        if (user is not null)
        {
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepo.UpdateAsync(user, uow.Session, ct);
        }

        await activityLogRepo.WriteAsync(
            targetType: "KolProfiles",
            targetId: cmd.KolId,
            actorUserId: currentUser.UserId,
            action: "Unsuspend",
            note: null,
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
