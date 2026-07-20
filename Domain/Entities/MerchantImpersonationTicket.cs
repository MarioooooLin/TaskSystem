namespace Domain.Entities;

/// <summary>
/// 管理者代理登入業者端的一次性票證。
/// 資料庫只儲存 TokenHash，不儲存明文 token。
/// </summary>
public class MerchantImpersonationTicket
{
    public long Id { get; set; }

    /// <summary>SHA-256 hash of the plaintext token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public long MerchantId { get; set; }

    /// <summary>執行代理登入的管理者 UserId。</summary>
    public long AdminUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>首次成功兌換的 UTC 時間；NULL 代表尚未兌換。</summary>
    public DateTime? UsedAtUtc { get; set; }

    public string? CreatedIp { get; set; }

    public string? UserAgent { get; set; }
}
