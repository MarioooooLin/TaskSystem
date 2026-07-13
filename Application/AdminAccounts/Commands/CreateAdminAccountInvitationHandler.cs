using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Net.Mail;

namespace Application.AdminAccounts.Commands;

public sealed class CreateAdminAccountInvitationHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        CreateAdminAccountInvitationCommand cmd,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure(Errors.AdminAccount.InvalidEmail);

        if (!IsValidEmail(cmd.Email))
            return Result.Failure(Errors.AdminAccount.InvalidEmail);

        await using var uow = await unitOfWork.BeginAsync(ct);

        var roles = await adminAccountRepo.GetActiveSystemRolesAsync(uow.Session, ct);
        if (!roles.Any(r => r.Id == cmd.RoleId))
            return Result.Failure(Errors.AdminAccount.RoleNotFound);

        var existingUser = await adminAccountRepo.GetUserByEmailAsync(cmd.Email.Trim(), uow.Session, ct);
        if (existingUser is not null)
            return Result.Failure(Errors.User.EmailAlreadyExists);

        var user = new User
        {
            AccountType = AccountType.Admin,
            Name = cmd.Name.Trim(),
            Email = cmd.Email.Trim().ToLowerInvariant(),
            PasswordHash = null,
            Status = UserStatus.Active,
        };

        var userId = await adminAccountRepo.InsertUserAsync(user, uow.Session, ct);

        await adminAccountRepo.UpsertProfileAsync(
            userId,
            cmd.Department,
            cmd.JobTitle,
            cmd.Phone,
            cmd.Note,
            uow.Session,
            ct);

        await adminAccountRepo.ReplaceRolesAsync(userId, [cmd.RoleId], uow.Session, ct);

        var token = GenerateToken();
        var invitation = new UserInvitation
        {
            UserId = userId,
            Email = user.Email,
            InvitedByUserId = currentUser.UserId,
            RoleId = cmd.RoleId,
            TokenHash = token,
            AccountType = AccountType.Admin,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48)
        };

        await adminAccountRepo.InsertInvitationAsync(invitation, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "AdminAccounts",
            targetId: userId,
            actorUserId: currentUser.UserId,
            action: "CreateAdminAccountInvitation",
            note: $"建立後台帳號邀請，角色：{roles.First(r => r.Id == cmd.RoleId).Name}",
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

    private static string GenerateToken()
        => Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Convert.ToHexString(Guid.NewGuid().ToByteArray());
}
