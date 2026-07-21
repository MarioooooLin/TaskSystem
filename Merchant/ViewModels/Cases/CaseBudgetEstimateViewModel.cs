namespace Merchant.ViewModels.Cases;

/// <summary>案件編輯頁預算預估側邊欄。</summary>
public sealed class CaseBudgetEstimateViewModel
{
    public decimal RewardAmountPerKol { get; set; }

    public int WantedKolCount { get; set; }

    public decimal RewardSubtotal { get; set; }

    public decimal CaseOpeningFee { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal PlatformServiceFee { get; set; }

    public decimal EstimatedFrozenAmount { get; set; }

    public decimal CurrentWalletBalance { get; set; }

    public decimal UsageRate => CurrentWalletBalance > 0
        ? Math.Min(EstimatedFrozenAmount / CurrentWalletBalance * 100m, 100m)
        : 0m;
}
