namespace Domain.Entities;

/// <summary>
/// 對應 CaseBudgetSnapshots 資料表，保存案件發布當下的預算快照。
/// 案件後續修改不追溯改寫已發布快照。
/// </summary>
public sealed class CaseBudgetSnapshot
{
    public long Id { get; set; }
    public long CaseId { get; set; }

    /// <summary>單人/組 KOL 現金報酬。</summary>
    public decimal RewardAmountPerKol { get; set; }

    /// <summary>預計招募組數。</summary>
    public int WantedKolCount { get; set; }

    /// <summary>報酬小計 = RewardAmountPerKol * WantedKolCount。</summary>
    public decimal RewardSubtotal { get; set; }

    /// <summary>費用明細 JSON。</summary>
    public string FeeItems { get; set; } = string.Empty;

    /// <summary>預計凍結總金額。</summary>
    public decimal EstimatedFrozenAmount { get; set; }

    /// <summary>發布當下系統參數快照 JSON。</summary>
    public string SettingsSnapshot { get; set; } = string.Empty;

    /// <summary>冪等性識別鍵，防止重複發布造成重複鎖款。</summary>
    public string? IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }
}
