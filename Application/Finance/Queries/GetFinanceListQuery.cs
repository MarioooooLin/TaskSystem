using Common.Pagination;
using Domain.Enums;

namespace Application.Finance.Queries;

/// <summary>帳務總覽列表分頁查詢。</summary>
public sealed record GetFinanceListQuery(
    string? Keyword,
    CaseStatus? Status,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
