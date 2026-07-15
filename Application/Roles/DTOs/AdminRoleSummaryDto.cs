namespace Application.Roles.DTOs;

/// <summary>後台角色管理 KPI 摘要。</summary>
public sealed class AdminRoleSummaryDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }
    public int DisabledCount { get; init; }
    public int InUseAccountCount { get; init; }
    public int HighRiskRoleCount { get; init; }
}
