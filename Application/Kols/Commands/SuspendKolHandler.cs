using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Kols.Commands;

public sealed class SuspendKolHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo,
    IUserRepository userRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(SuspendKolCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var kol = await kolRepo.GetByIdAsync(cmd.KolId, uow.Session, ct);
        if (kol is null)
            return Result.Failure(Errors.Kol.NotFound);

        if (kol.VerificationStatus == VerificationStatus.Suspended)
            return Result.Failure(Errors.Kol.AlreadySuspended);

        kol.VerificationStatus = VerificationStatus.Suspended;
        kol.VerifiedAt = DateTime.UtcNow;
        kol.VerifiedByAdminId = currentUser.UserId;
        kol.SuspensionNote = cmd.SuspensionNote;
        kol.UpdatedAt = DateTime.UtcNow;

        await kolRepo.UpdateAsync(kol, uow.Session, ct);

        // 同步停止 KOL 登入帳號
        var user = await userRepo.GetByIdAsync(kol.UserId, uow.Session, ct);
        if (user is not null)
        {
            user.Status = UserStatus.Suspended;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepo.UpdateAsync(user, uow.Session, ct);
        }

        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
