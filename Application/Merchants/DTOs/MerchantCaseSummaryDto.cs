using Domain.Enums;

namespace Application.Merchants.DTOs;

/// <summary>業者詳情頁的案件列表列項（近期 10 筆）。</summary>
public sealed class MerchantCaseSummaryDto
{
    public long CaseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public CaseStatus Status { get; init; }
    public decimal CashRewardAmount { get; init; }
    public int WantedKolCount { get; init; }
    public int ApprovedAssignmentCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
