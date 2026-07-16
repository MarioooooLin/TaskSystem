using Application.Abstractions.Notifications;
using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.AdminAccounts.Options;
using Common.Errors;
using Common.Results;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using System.Net;
using System.Net.Mail;

namespace Application.AdminAccounts.Commands;

public sealed class CreateAdminAccountInvitationHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser,
    InvitationEmailOptions invitationOptions,
    IEmailSender emailSender)
{
    public async Task<Result> HandleAsync(
        CreateAdminAccountInvitationCommand cmd,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result.Failure(Errors.AdminAccount.InvalidEmail);

        if (!IsValidEmail(cmd.Email))
            return Result.Failure(Errors.AdminAccount.InvalidEmail);

        if (cmd.RoleIds is null || cmd.RoleIds.Length == 0)
            return Result.Failure(Errors.AdminAccount.RoleRequired);

        await using var uow = await unitOfWork.BeginAsync(ct);

        var roles = await adminAccountRepo.GetActiveSystemRolesAsync(uow.Session, ct);
        if (cmd.RoleIds.Any(id => !roles.Any(r => r.Id == id)))
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

        await adminAccountRepo.ReplaceRolesAsync(userId, cmd.RoleIds, uow.Session, ct);

        var primaryRoleId = cmd.RoleIds.First();
        var token = GenerateToken();
        var invitation = new UserInvitation
        {
            UserId = userId,
            Email = user.Email,
            InvitedByUserId = currentUser.UserId,
            RoleId = primaryRoleId,
            TokenHash = token,
            AccountType = AccountType.Admin,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48)
        };

        await adminAccountRepo.InsertInvitationAsync(invitation, uow.Session, ct);

        var sendResult = await SendInvitationEmailAsync(user.Name, user.Email, token, ct);
        if (sendResult.IsFailure)
            return sendResult;

        var roleNames = string.Join("。", cmd.RoleIds.Select(id => roles.First(r => r.Id == id).Name));

        await activityLogRepo.WriteAsync(
            targetType: "AdminAccounts",
            targetId: userId,
            actorUserId: currentUser.UserId,
            action: "CreateAdminAccountInvitation",
            note: $"建立後台帳號邀請，角色：{roleNames}",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }

    private async Task<Result> SendInvitationEmailAsync(
        string name,
        string email,
        string token,
        CancellationToken ct)
    {
        try
        {
            var baseUrl = (invitationOptions.BaseUrl ?? string.Empty).TrimEnd('/');
            var link = $"{baseUrl}/Account/SetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
            var subject = "任務系統管理者後台 - 帳號開通邀請";
            var body = $@"<p>Hi {WebUtility.HtmlEncode(name)},</p>
<p>您已被邀請加入台灣旅圖任務系統管理者後台。請點擊下方連結設定密碼：</p>
<p><a href=""{link}"">設定密碼</a></p>
<p>此連結將於 48 小時後失效。</p>";

            await emailSender.SendAsync(new EmailMessage(email, subject, body), ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Problem("Email.SendFailed", $"郵件發送失敗：{ex.Message}"));
        }
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
