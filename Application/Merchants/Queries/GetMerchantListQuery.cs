using Common.Pagination;
using Domain.Enums;

namespace Application.Merchants.Queries;

/// <summary>業者列表分頁查詢（Admin 後台）。</summary>
public sealed record GetMerchantListQuery(
    string? Keyword,
    VerificationStatus? VerificationStatus,
    string? IndustryType,
    DateTime? DateFrom,
    bool? HasCredit,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
