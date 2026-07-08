using Common.Pagination;

namespace Application.Kols.Queries;

/// <summary>待審核 KOL 列表分頁查詢（ADM-015）。</summary>
public sealed record GetKolReviewListQuery(
    string? Keyword,
    /// <summary>pending / resubmit / returned / approved。null = 全部 Pending。</summary>
    string? StatusFilter,
    short? Category,
    short? Platform,
    DateOnly? SubmittedDate,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
