using Common.Pagination;
using Domain.Enums;

namespace Application.Cases.Queries;

/// <summary>業者端案件管理列表分頁查詢。</summary>
public sealed record GetMerchantCaseListQuery(
    long MerchantId,
    string? Keyword,
    CaseStatus? Status,
    bool? ClosedOnly,
    int? RewardTypeFilter,
    SocialPlatform? Platform,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 10)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
