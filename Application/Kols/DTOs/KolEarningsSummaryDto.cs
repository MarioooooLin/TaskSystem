namespace Application.Kols.DTOs;

/// <summary>KOL 收益概況 DTO（詳情頁收益區塊）。</summary>
public sealed class KolEarningsSummaryDto
{
    public decimal PendingAmount { get; init; }
    public decimal AvailableAmount { get; init; }
    public decimal PaidAmount { get; init; }

    /// <summary>待請款撥款總計（Requested 狀態）。</summary>
    public decimal RequestedAmount { get; init; }
}
