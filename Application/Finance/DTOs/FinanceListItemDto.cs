using Domain.Enums;

namespace Application.Finance.DTOs;

/// <summary>帳務總覽列表列項 DTO。</summary>
public sealed class FinanceListItemDto
{
    public long CaseId { get; init; }
    public string CaseNo { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;

    public long MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;

    public CaseStatus Status { get; init; }

    /// <summary>案件結算日期。</summary>
    public DateTime? SettledAt { get; init; }

    /// <summary>收入總金額（業者結算/儲值口徑）。</summary>
    public decimal IncomeAmount { get; init; }

    /// <summary>支出資金（KOL 淨付金額彙總）。</summary>
    public decimal ExpenseAmount { get; init; }

    /// <summary>是否存在未結案異議。</summary>
    public bool HasOpenDispute { get; init; }

    /// <summary>是否啟用導購分潤。</summary>
    public bool IsCommissionEnabled { get; init; }

    /// <summary>可展開的 KOL 任務帳務明細。</summary>
    public IReadOnlyList<FinanceTaskDetailDto> TaskDetails { get; init; } = Array.Empty<FinanceTaskDetailDto>();
}
