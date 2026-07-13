using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.AdminAccounts.Commands;

public sealed class SuspendAdminAccountHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        SuspendAdminAccountCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var user = await adminAccountRepo.GetUserByEmailAsync(
            (await adminAccountRepo.GetByIdAsync(cmd.UserId, uow.Session, ct))?.Email ?? string.Empty,
            uow.Session,
            ct);

        if (user is null || user.AccountType != AccountType.Admin)
            return Result.Failure(Errors.AdminAccount.NotFound);

        if (user.Status == UserStatus.Suspended)
            return Result.Failure(Errors.AdminAccount.AlreadySuspended);

        // 保護最後一個系統管理者（簡化：Active Admin 僅剩一人時不可停用）
        var lastAdminId = await adminAccountRepo.GetLastActiveSystemAdminUserIdAsync(uow.Session, ct);
        if (lastAdminId == user.Id)
            return Result.Failure(Errors.User.LastSystemAdmin);

        user.Status = UserStatus.Suspended;
        await adminAccountRepo.UpdateUserAsync(user, uow.Session, ct);
        await adminAccountRepo.CancelInvitationsByUserAsync(user.Id, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "AdminAccounts",
            targetId: user.Id,
            actorUserId: currentUser.UserId,
            action: "SuspendAdminAccount",
            note: "停用後台帳號",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
