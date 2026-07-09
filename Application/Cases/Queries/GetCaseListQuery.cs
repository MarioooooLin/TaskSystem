using Common.Pagination;
using Domain.Enums;

namespace Application.Cases.Queries;

/// <summary>案件監控列表分頁查詢。</summary>
public sealed record GetCaseListQuery(
    string? Keyword,
    CaseStatus? Status,
    bool? HasPendingReview,
    bool? HasCommission,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
