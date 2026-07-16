namespace Application.Kols.DTOs;

/// <summary>社群帳號列項 DTO（KOL 詳情頁社群區塊）。</summary>
public sealed class KolSocialAccountDto
{
    public long Id { get; init; }

    /// <summary>1=X 2=IG 3=FB 4=YT 5=Blog 6=小紅書 7=TikTok 8=抖音 9=Threads 10=Snapchat 11=WeChat</summary>
    public short Platform { get; init; }

    public string AccountName { get; init; } = string.Empty;
    public int? FollowersCount { get; init; }

    /// <summary>1=ApiSync 2=ManualInput</summary>
    public short DataSource { get; init; }

    /// <summary>1=Verified 2=Unverified 3=NeedsConfirmation</summary>
    public short VerificationStatus { get; init; }

    public DateTime? LastSyncAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
