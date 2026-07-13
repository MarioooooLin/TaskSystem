namespace Application.AdminAccounts.Commands;

/// <summary>重新寄送後台帳號邀請。</summary>
public sealed record ResendAdminAccountInvitationCommand(long UserId);
