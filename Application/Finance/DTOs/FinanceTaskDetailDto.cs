namespace Application.Finance.DTOs;

/// <summary>帳務總覽頁可展開的單一 KOL 任務帳務明細。</summary>
public sealed class FinanceTaskDetailDto
{
    public long CaseId { get; init; }
    public long TaskId { get; init; }
    public long KolId { get; init; }
    public string KolName { get; init; } = string.Empty;

    /// <summary>現金任務報酬（Mission）。</summary>
    public decimal MissionAmount { get; init; }

    /// <summary>導購佣金（Affiliate）。</summary>
    public decimal AffiliateAmount { get; init; }

    /// <summary>稅金/手續費。</summary>
    public decimal TaxFeeAmount { get; init; }

    /// <summary>淨付金額。</summary>
    public decimal NetAmount { get; init; }

    /// <summary>收益狀態文字。</summary>
    public string StatusText { get; init; } = string.Empty;
}
