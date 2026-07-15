using Common.Pagination;

namespace Application.Roles.DTOs;

/// <summary>後台角色管理列表查詢結果。</summary>
public sealed class AdminRoleListResultDto
{
    public PagedResult<AdminRoleListItemDto> List { get; init; } = null!;
    public AdminRoleSummaryDto Summary { get; init; } = new();
}
