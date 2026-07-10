using Application.Finance.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.Finance;

/// <summary>帳務總覽首頁 ViewModel。</summary>
public sealed class FinanceIndexViewModel
{
    public FinanceSummaryDto Summary { get; init; } = new();
    public PagedResult<FinanceListItemDto> List { get; init; } = PagedResult<FinanceListItemDto>.Empty(1, 20);
    public FinanceListQueryViewModel Query { get; init; } = new();
}
