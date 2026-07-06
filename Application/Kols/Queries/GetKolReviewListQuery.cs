using Common.Pagination;
using Domain.Enums;

namespace Application.Kols.Queries;

/// <summary>待審核 KOL 列表分頁查詢（ADM-015）。</summary>
public sealed record GetKolReviewListQuery(
    string? Keyword,
    VerificationStatus? VerificationStatus,
    short? Category,
    short? Platform,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
