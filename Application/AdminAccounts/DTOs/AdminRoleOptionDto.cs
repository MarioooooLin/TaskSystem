namespace Application.AdminAccounts.DTOs;

/// <summary>後台可指派的系統角色選項。</summary>
public sealed class AdminRoleOptionDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}
