namespace Application.Merchants.DTOs;

/// <summary>業者折扣金錢包概況（詳情頁顯示用）。</summary>
public sealed class MerchantCreditWalletSummaryDto
{
    /// <summary>可用折扣金餘額。</summary>
    public decimal AvailableAmount { get; init; }

    /// <summary>本月已使用折扣金（從 MerchantCreditTransactions 統計）。</summary>
    public decimal MonthlyUsedAmount { get; init; }
}
