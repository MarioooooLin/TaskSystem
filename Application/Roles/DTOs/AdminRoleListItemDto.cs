namespace Application.Roles.DTOs;

/// <summary>後台角色管理列表項目。</summary>
public sealed class AdminRoleListItemDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int UsageCount { get; init; }
    public bool IsActive { get; init; }
    public bool IsSystemReserved { get; init; }
    public bool HasHighRiskPermission { get; init; }
    public DateTime UpdatedAt { get; init; }
}
