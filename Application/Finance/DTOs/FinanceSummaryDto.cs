namespace Application.Finance.DTOs;

/// <summary>帳務總覽頁平台財務摘要。</summary>
public sealed class FinanceSummaryDto
{
    /// <summary>平台總收入（營運收入口徑）。</summary>
    public decimal TotalIncome { get; init; }

    /// <summary>平台總支出（KOL 支出與退款調整口徑）。</summary>
    public decimal TotalExpense { get; init; }

    /// <summary>平台毛利 = TotalIncome - TotalExpense。</summary>
    public decimal GrossProfit { get; init; }

    /// <summary>資金流入（業者線下儲值），獨立於營運收入。</summary>
    public decimal FundInflow { get; init; }
}
