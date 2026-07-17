using Application.Cases.DTOs;
using Common.Pagination;

namespace Merchant.ViewModels.Cases;

/// <summary>業者端案件管理首頁 ViewModel。</summary>
public sealed class CaseIndexViewModel
{
    public MerchantCaseSummaryDto Summary { get; init; } = new();
    public PagedResult<MerchantCaseListItemDto> List { get; init; } = PagedResult<MerchantCaseListItemDto>.Empty(1, 10);
    public CaseListQueryViewModel Query { get; init; } = new();
}
