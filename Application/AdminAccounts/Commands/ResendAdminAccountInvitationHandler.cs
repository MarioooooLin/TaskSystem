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

namespace Application.AdminAccounts.Commands;

public sealed class ResendAdminAccountInvitationHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser,
    InvitationEmailOptions invitationOptions,
    IEmailSender emailSender)
{
    public async Task<Result> HandleAsync(
        ResendAdminAccountInvitationCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var existing = await adminAccountRepo.GetByIdAsync(cmd.UserId, uow.Session, ct);
        if (existing is null)
            return Result.Failure(Errors.AdminAccount.NotFound);

        var user = await adminAccountRepo.GetUserByEmailAsync(existing.Email, uow.Session, ct);
        if (user is null || user.AccountType != AccountType.Admin)
            return Result.Failure(Errors.AdminAccount.NotFound);

        // 若已有密碼代表邀請已完成
        if (!string.IsNullOrEmpty(user.PasswordHash))
            return Result.Failure(Errors.AdminAccount.InvitationAlreadyAccepted);

        // 將舊邀請取消
        await adminAccountRepo.CancelInvitationsByUserAsync(user.Id, uow.Session, ct);

        var roles = await adminAccountRepo.GetActiveSystemRolesAsync(uow.Session, ct);
        var userRoleIds = existing.RoleIds.Intersect(roles.Select(r => r.Id)).ToList();
        var roleId = userRoleIds.FirstOrDefault();

        var token = Convert.ToHexString(Guid.NewGuid().ToByteArray()) + Convert.ToHexString(Guid.NewGuid().ToByteArray());
        var invitation = new UserInvitation
        {
            UserId = user.Id,
            Email = user.Email,
            InvitedByUserId = currentUser.UserId,
            RoleId = roleId == 0 ? null : roleId,
            TokenHash = token,
            AccountType = AccountType.Admin,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddHours(48)
        };

        await adminAccountRepo.InsertInvitationAsync(invitation, uow.Session, ct);

        var sendResult = await SendInvitationEmailAsync(user.Name, user.Email, token, ct);
        if (sendResult.IsFailure)
            return sendResult;

        await activityLogRepo.WriteAsync(
            targetType: "AdminAccounts",
            targetId: user.Id,
            actorUserId: currentUser.UserId,
            action: "ResendAdminAccountInvitation",
            note: "重新寄送後台帳號邀請",
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
            var subject = "任務系統管理者後台 - 帳號開通邀請（重發）";
            var body = $@"<p>Hi {WebUtility.HtmlEncode(name)},</p>
<p>請點擊下方連結設定您的後台帳號密碼：</p>
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
}
