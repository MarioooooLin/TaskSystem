using Application.Merchants.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.Merchant;

/// <summary>業者管理首頁 ViewModel（列表 + KPI 摘要 + 篩選條件）。</summary>
public sealed class MerchantIndexViewModel
{
    public PagedResult<MerchantListItemDto> List { get; init; } = null!;
    public MerchantSummaryDto Summary { get; init; } = new();
    public MerchantListQueryViewModel Query { get; init; } = new();
}
