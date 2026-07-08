using Application.Kols.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.Kol;

/// <summary>審核新進 KOL 列表頁 ViewModel（列表 + KPI 摘要 + 篩選條件）。</summary>
public sealed class KolReviewIndexViewModel
{
    public PagedResult<KolReviewListItemDto> List { get; init; } = null!;
    public KolReviewSummaryDto Summary { get; init; } = new();
    public KolReviewListQueryViewModel Query { get; init; } = new();
}
