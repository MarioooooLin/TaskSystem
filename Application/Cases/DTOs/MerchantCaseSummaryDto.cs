namespace Application.Cases.DTOs;

/// <summary>業者端案件管理頁狀態統計。</summary>
public sealed class MerchantCaseSummaryDto
{
    public int TotalCount { get; init; }
    public int DraftCount { get; init; }
    public int RecruitingCount { get; init; }
    public int InProgressCount { get; init; }
    public int PendingAcceptanceCount { get; init; }
    public int ClosedCount { get; init; }
}
