namespace Application.Dashboard.DTOs;

/// <summary>營運總覽頁右側時效異常監控。</summary>
public sealed class DashboardAlertDto
{
    public int OverdueCaseCount { get; init; }
    public int ReviewOverdueCount { get; init; }
    public int AffiliateSyncErrorCount { get; init; }
}
