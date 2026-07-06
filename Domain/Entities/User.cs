using Domain.Enums;

namespace Domain.Entities;

public class User
{
    public long Id { get; set; }
    public AccountType AccountType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }  // 第三方登入可為 null
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsActive() => Status == UserStatus.Active;
}
