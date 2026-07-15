namespace Application.Roles.DTOs;

/// <summary>後台角色編輯頁資料。</summary>
public sealed class AdminRolePermissionEditDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsSystemReserved { get; init; }
    public IReadOnlyList<long> SelectedPermissionIds { get; init; } = [];
    public IReadOnlyList<AdminPermissionDto> Permissions { get; init; } = [];
}
