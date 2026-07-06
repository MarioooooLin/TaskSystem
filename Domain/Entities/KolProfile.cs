using Domain.Enums;

namespace Domain.Entities;

public class KolProfile
{
    public long Id { get; set; }
    public long UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? LineContactId { get; set; }
    public string? Intro { get; set; }

    public bool AcceptsCash { get; set; } = true;
    public bool AcceptsBarter { get; set; } = true;
    public bool AcceptsCommission { get; set; } = true;

    /// <summary>快取值，實際數字以 KolSocialAccounts 為準。</summary>
    public int? FollowersCount { get; set; }

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public DateTime? VerifiedAt { get; set; }
    public long? VerifiedByAdminId { get; set; }

    /// <summary>審核退回原因（顯示於審核詳情頁）。</summary>
    public string? RejectionNote { get; set; }

    /// <summary>停權原因。</summary>
    public string? SuspensionNote { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
