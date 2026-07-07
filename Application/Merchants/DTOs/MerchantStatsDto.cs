namespace Application.Merchants.DTOs;

/// <summary>業者統計數字（案件數、執行中、已完成、爭議數、完成率）。</summary>
public sealed class MerchantStatsDto
{
    public int CaseCount { get; init; }
    public int InProgressCount { get; init; }
    public int CompletedCount { get; init; }
    public int DisputeCount { get; init; }

    /// <summary>完成率 = Completed Tasks / 總綁定 Tasks，0～100。</summary>
    public decimal CompletionRate { get; init; }
}
