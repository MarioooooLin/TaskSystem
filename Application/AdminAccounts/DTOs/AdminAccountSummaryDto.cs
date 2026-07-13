namespace Application.AdminAccounts.DTOs;

/// <summary>後台帳號管理 KPI 摘要。</summary>
public sealed class AdminAccountSummaryDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int PendingInvitationCount { get; init; }
    public int SuspendedCount { get; init; }
    public int ExpiredInvitationCount { get; init; }
}
