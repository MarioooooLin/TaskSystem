namespace Application.Roles.Commands;

/// <summary>建立後台系統角色。</summary>
public sealed record CreateAdminRoleCommand(
    string Name,
    string? Description,
    bool IsSystemReserved,
    IReadOnlyList<long> PermissionIds);
