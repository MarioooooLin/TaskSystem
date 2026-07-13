namespace Application.AdminAccounts.DTOs;

/// <summary>後台帳號管理最近異動紀錄 DTO。</summary>
public sealed class AdminAccountLogItemDto
{
    public DateTime CreatedAt { get; init; }
    public string ActorName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string TargetName { get; init; } = string.Empty;
    public string? Note { get; init; }
}
