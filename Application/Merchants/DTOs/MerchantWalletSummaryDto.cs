namespace Application.Merchants.DTOs;

/// <summary>業者錢包概況（詳情頁上半區塊）。</summary>
public sealed class MerchantWalletSummaryDto
{
    public decimal AvailableAmount { get; init; }
    public decimal FrozenAmount { get; init; }
    public decimal TotalDepositedAmount { get; init; }
}
