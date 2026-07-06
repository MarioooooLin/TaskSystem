namespace Application.Kols.DTOs;

/// <summary>KOL 任務摘要列項（詳情頁任務紀錄區塊）。</summary>
public sealed class KolTaskSummaryDto
{
    public long TaskId { get; init; }
    public long CaseId { get; init; }
    public string CaseTitle { get; init; } = string.Empty;
    public short TaskStatus { get; init; }
    public string? MerchantName { get; init; }
    public decimal CashRewardAmount { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
