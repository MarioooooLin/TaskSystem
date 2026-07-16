using Domain.Enums;

namespace Domain.Entities;

public class KolSocialAccount
{
    public long Id { get; set; }
    public long KolId { get; set; }

    /// <summary>1=X 2=IG 3=FB 4=YT 5=Blog 6=小紅書 7=TikTok 8=抖音 9=Threads 10=Snapchat 11=WeChat</summary>
    public short Platform { get; set; }

    public string AccountName { get; set; } = string.Empty;
    public int? FollowersCount { get; set; }

    /// <summary>1=ApiSync 2=ManualInput</summary>
    public short DataSource { get; set; } = 2;

    /// <summary>1=Verified 2=Unverified 3=NeedsConfirmation</summary>
    public short VerificationStatus { get; set; } = 2;

    public DateTime? LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
