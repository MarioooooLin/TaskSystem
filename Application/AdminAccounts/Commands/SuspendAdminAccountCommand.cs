namespace Application.AdminAccounts.Commands;

/// <summary>停用後台帳號。</summary>
public sealed record SuspendAdminAccountCommand(long UserId);
