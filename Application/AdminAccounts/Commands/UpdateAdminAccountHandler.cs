using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;
using System.Net.Mail;

namespace Application.AdminAccounts.Commands;

public sealed class UpdateAdminAccountHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        UpdateAdminAccountCommand cmd,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure(Errors.AdminAccount.InvalidEmail);

        if (!IsValidEmail(cmd.Email))
            return Result.Failure(Errors.AdminAccount.InvalidEmail);

        if (cmd.RoleIds.Length == 0)
            return Result.Failure(Errors.AdminAccount.RoleRequired);

        await using var uow = await unitOfWork.BeginAsync(ct);

        var existing = await adminAccountRepo.GetByIdAsync(cmd.UserId, uow.Session, ct);
        if (existing is null)
            return Result.Failure(Errors.AdminAccount.NotFound);

        var roles = await adminAccountRepo.GetActiveSystemRolesAsync(uow.Session, ct);
        if (cmd.RoleIds.Any(id => !roles.Any(r => r.Id == id)))
            return Result.Failure(Errors.AdminAccount.RoleNotFound);

        var sameEmailUser = await adminAccountRepo.GetUserByEmailAsync(cmd.Email.Trim(), uow.Session, ct);
        if (sameEmailUser is not null && sameEmailUser.Id != cmd.UserId)
            return Result.Failure(Errors.User.EmailAlreadyExists);

        var user = await adminAccountRepo.GetUserByEmailAsync(existing.Email, uow.Session, ct);
        if (user is null || user.AccountType != AccountType.Admin)
            return Result.Failure(Errors.AdminAccount.NotFound);

        user.Name = cmd.Name.Trim();
        user.Email = cmd.Email.Trim().ToLowerInvariant();

        if (cmd.Status == UserStatus.Suspended && user.Status != UserStatus.Suspended)
        {
            var lastAdminId = await adminAccountRepo.GetLastActiveSystemAdminUserIdAsync(uow.Session, ct);
            if (lastAdminId == user.Id)
                return Result.Failure(Errors.User.LastSystemAdmin);

            await adminAccountRepo.CancelInvitationsByUserAsync(user.Id, uow.Session, ct);
        }

        user.Status = cmd.Status;
        await adminAccountRepo.UpdateUserAsync(user, uow.Session, ct);

        await adminAccountRepo.UpsertProfileAsync(
            cmd.UserId,
            cmd.Department,
            cmd.JobTitle,
            cmd.Phone,
            cmd.Note,
            uow.Session,
            ct);

        await adminAccountRepo.ReplaceRolesAsync(cmd.UserId, cmd.RoleIds, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "AdminAccounts",
            targetId: cmd.UserId,
            actorUserId: currentUser.UserId,
            action: "UpdateAdminAccount",
            note: "更新後台帳號基本資料與角色",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            _ = new MailAddress(email.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
