namespace Application.Kols.DTOs;

/// <summary>KOL 統計數字（詳情頁頂部卡片）。</summary>
public sealed class KolStatsDto
{
    public int TaskCount { get; init; }
    public int CompletedTaskCount { get; init; }
    public int PendingReviewCount { get; init; }
    public int DisputeCount { get; init; }
    public int AbandonedTaskCount { get; init; }
}
