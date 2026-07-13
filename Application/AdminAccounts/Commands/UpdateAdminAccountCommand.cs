namespace Application.AdminAccounts.Commands;

/// <summary>更新後台帳號資料。</summary>
public sealed record UpdateAdminAccountCommand(
    long UserId,
    string Name,
    string Email,
    long[] RoleIds,
    string? Department,
    string? JobTitle,
    string? Phone,
    string? Note);
