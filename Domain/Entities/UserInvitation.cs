using Domain.Enums;

namespace Domain.Entities;

public class UserInvitation
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public long InvitedByUserId { get; set; }
    public long? RoleId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public AccountType AccountType { get; set; } = AccountType.Admin;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsExpired() => Status == InvitationStatus.Pending && ExpiresAt < DateTime.UtcNow;
}
