using Application.Kols.DTOs;

namespace Application.Dashboard.DTOs;

/// <summary>營運總覽頁完整資料。</summary>
public sealed class DashboardDto
{
    public DashboardKpiDto Kpi { get; init; } = new();
    public DashboardAlertDto Alerts { get; init; } = new();
    public IReadOnlyList<KolReviewListItemDto> PendingKolReviews { get; init; } = [];
    public IReadOnlyList<DashboardDisputeItemDto> RecentDisputes { get; init; } = [];
}
