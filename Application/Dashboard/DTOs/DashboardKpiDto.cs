namespace Application.Dashboard.DTOs;

/// <summary>營運總覽頁頂部 KPI 卡片。</summary>
public sealed class DashboardKpiDto
{
    public int ActiveMerchantCount { get; init; }
    public int ActiveKolCount { get; init; }
    public int InProgressCaseCount { get; init; }
    public int DisputeCount { get; init; }
    public decimal PendingPayoutAmount { get; init; }
}
