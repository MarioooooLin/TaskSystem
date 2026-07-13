using Common.Pagination;
using Domain.Enums;

namespace Application.AdminAccounts.Queries;

/// <summary>後台帳號列表分頁查詢。</summary>
public sealed record GetAdminAccountListQuery(
    string? Keyword,
    UserStatus? Status,
    string? Department,
    long? RoleId,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
