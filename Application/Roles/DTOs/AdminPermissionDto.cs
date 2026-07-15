namespace Application.Roles.DTOs;

/// <summary>後台權限項目選項。</summary>
public sealed class AdminPermissionDto
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public short RiskLevel { get; init; }
    public string GroupName { get; init; } = string.Empty;
}
