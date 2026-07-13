using Domain.Enums;

namespace Application.AdminAccounts.DTOs;

/// <summary>後台帳號列表列項 DTO（Admin 後台用）。</summary>
public sealed class AdminAccountListItemDto
{
    public long UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Department { get; init; }
    public string RolesDisplay { get; init; } = string.Empty;
    public UserStatus Status { get; init; }
    public bool HasPendingInvitation { get; init; }
    public bool IsInvitationExpired { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
