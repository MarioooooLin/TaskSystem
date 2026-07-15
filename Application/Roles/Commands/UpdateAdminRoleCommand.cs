namespace Application.Roles.Commands;

/// <summary>更新後台系統角色與權限。</summary>
public sealed record UpdateAdminRoleCommand(
    long RoleId,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<long> PermissionIds);
