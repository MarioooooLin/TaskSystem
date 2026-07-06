using Common.Pagination;
using Domain.Enums;

namespace Application.Kols.Queries;

/// <summary>KOL 列表分頁查詢（ADM-005）。</summary>
public sealed record GetKolListQuery(
    string? Keyword,
    VerificationStatus? VerificationStatus,
    short? Category,
    short? Platform,
    bool? HasBankAccount,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
