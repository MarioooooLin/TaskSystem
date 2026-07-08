namespace Application.Merchants.DTOs;

/// <summary>業者管理 KPI 摘要（全部/啟用中/停用中 計數）。</summary>
public sealed class MerchantSummaryDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int SuspendedCount { get; init; }
}
