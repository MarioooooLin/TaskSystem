namespace Application.Kols.DTOs;

/// <summary>審核新進 KOL 頁面專用 KPI 摘要（ADM-015）。</summary>
public sealed class KolReviewSummaryDto
{
    /// <summary>待審核（首次，VerificationStatus = 1 且無重送審核記錄）。</summary>
    public int PendingCount { get; init; }

    /// <summary>重送審核（VerificationStatus = 1 且有 KolReviewEvents.ActionType = 2）。</summary>
    public int ResubmitCount { get; init; }

    /// <summary>已退回待補（VerificationStatus = 3）。</summary>
    public int ReturnedCount { get; init; }

    /// <summary>今日新增（UpdatedAt = TODAY，VerificationStatus IN (1, 3)）。</summary>
    public int TodayNewCount { get; init; }

    /// <summary>超過 3 日未審核（VerificationStatus = 1 且 UpdatedAt 距今超過 3 天）。</summary>
    public int OverdueCount { get; init; }
}
