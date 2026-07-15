using Common.Pagination;

namespace Application.Roles.Queries;

/// <summary>後台角色管理列表分頁查詢。</summary>
public sealed record GetAdminRoleListQuery(
    string? Keyword,
    bool? IsActive,
    bool? IsSystemReserved,
    bool? HasHighRiskPermission,
    int Page = 1,
    int PageSize = 20)
{
    public PageQuery ToPageQuery() => new(Page, PageSize);
}
