using Application.Roles.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.RolePermission;

/// <summary>後台角色管理首頁 ViewModel（列表 + KPI 摘要 + 篩選條件）。</summary>
public sealed class AdminRoleIndexViewModel
{
    public PagedResult<AdminRoleListItemDto> List { get; init; } = null!;
    public AdminRoleSummaryDto Summary { get; init; } = new();
    public AdminRoleListQueryViewModel Query { get; init; } = new();
}
