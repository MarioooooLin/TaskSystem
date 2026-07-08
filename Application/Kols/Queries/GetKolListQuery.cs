using Common.Pagination;
using Domain.Enums;

namespace Application.Kols.Queries;

/// <summary>KOL 列表分頁查詢（ADM-005）。</summary>
public sealed record GetKolListQuery(
    string? Keyword,
    VerificationStatus? VerificationStatus,
    IReadOnlyList<short>? Categories,
    IReadOnlyList<short>? Platforms,
    bool? HasBankAccount,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
