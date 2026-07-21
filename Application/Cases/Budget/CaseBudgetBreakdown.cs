namespace Application.Cases.Budget;

/// <summary>案件發布前的預算試算結果。</summary>
public sealed class CaseBudgetBreakdown
{
    /// <summary>單人 / 組 KOL 現金報酬。</summary>
    public decimal RewardAmountPerKol { get; init; }

    /// <summary>預計招募組數。</summary>
    public int WantedKolCount { get; init; }

    /// <summary>報酬小計。</summary>
    public decimal RewardSubtotal { get; init; }

    /// <summary>案件開案費。</summary>
    public decimal CaseOpeningFee { get; init; }

    /// <summary>折扣金折抵金額。</summary>
    public decimal DiscountAmount { get; init; }

    /// <summary>平台服務費（以「報酬小計 + 開案費 - 折扣」為基礎）。</summary>
    public decimal PlatformServiceFee { get; init; }

    /// <summary>預計凍結總金額。</summary>
    public decimal EstimatedFrozenAmount { get; init; }

    /// <summary>費用明細 JSON，寫入 CaseBudgetSnapshots.FeeItems。</summary>
    public string FeeItemsJson { get; init; } = "[]";
}

/// <summary>單一費用明細項目。</summary>
public sealed class CaseFeeItem
{
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Note { get; init; }
}
