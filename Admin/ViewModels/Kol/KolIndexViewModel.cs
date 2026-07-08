using Application.Kols.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.Kol;

/// <summary>KOL 管理首頁 ViewModel（列表 + KPI 摘要 + 篩選條件）。</summary>
public sealed class KolIndexViewModel
{
    public PagedResult<KolListItemDto> List { get; init; } = null!;
    public KolSummaryDto Summary { get; init; } = new();
    public KolListQueryViewModel Query { get; init; } = new();
}
