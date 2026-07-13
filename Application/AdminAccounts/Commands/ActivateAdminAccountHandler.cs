using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.AdminAccounts.Commands;

public sealed class ActivateAdminAccountHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        ActivateAdminAccountCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var existing = await adminAccountRepo.GetByIdAsync(cmd.UserId, uow.Session, ct);
        if (existing is null)
            return Result.Failure(Errors.AdminAccount.NotFound);

        var user = await adminAccountRepo.GetUserByEmailAsync(existing.Email, uow.Session, ct);
        if (user is null || user.AccountType != AccountType.Admin)
            return Result.Failure(Errors.AdminAccount.NotFound);

        if (user.Status == UserStatus.Active)
            return Result.Failure(Errors.AdminAccount.AlreadyActive);

        user.Status = UserStatus.Active;
        await adminAccountRepo.UpdateUserAsync(user, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "AdminAccounts",
            targetId: user.Id,
            actorUserId: currentUser.UserId,
            action: "ActivateAdminAccount",
            note: "啟用後台帳號",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
