namespace Application.Cases.DTOs;

/// <summary>案件監控頁 KPI 統計數字。</summary>
public sealed class CaseSummaryDto
{
    public int TotalCount { get; init; }
    public int DraftCount { get; init; }
    public int RecruitingCount { get; init; }
    public int RecruitmentClosedCount { get; init; }
    public int InProgressCount { get; init; }
    public int CompletedCount { get; init; }
    public int CancelledCount { get; init; }
}
