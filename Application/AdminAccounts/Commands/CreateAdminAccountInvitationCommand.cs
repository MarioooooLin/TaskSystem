namespace Application.AdminAccounts.Commands;

/// <summary>建立後台帳號邀請。</summary>
public sealed record CreateAdminAccountInvitationCommand(
    string Name,
    string Email,
    long[] RoleIds,
    string? Department,
    string? JobTitle,
    string? Phone,
    string? Note);
