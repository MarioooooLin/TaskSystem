using Domain.Enums;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Infrastructure.Presentation;

public static class EnumDisplayExtensions
{
    public static string ToDisplayName(this VerificationStatus status)
        => status switch
        {
            VerificationStatus.Pending => "待審核",
            VerificationStatus.Approved => "啟用中",
            VerificationStatus.Rejected => "已退回",
            VerificationStatus.Suspended => "已停權",
            _ => status.ToString()
        };

    public static string ToDisplayName(this UserStatus status)
        => status switch
        {
            UserStatus.Active => "啟用中",
            UserStatus.Suspended => "停用中",
            UserStatus.Deleted => "已刪除",
            _ => status.ToString()
        };

    public static string ToDisplayName(this CaseStatus status)
        => status switch
        {
            CaseStatus.Draft => "草稿",
            CaseStatus.Recruiting => "招募中",
            CaseStatus.RecruitmentClosed => "招募截止",
            CaseStatus.InProgress => "執行中",
            CaseStatus.Completed => "已完成",
            CaseStatus.Settled => "已結算",
            CaseStatus.Cancelled => "已取消",
            _ => status.ToString()
        };

    public static string ToDisplayName(this TaskStatus status)
        => status switch
        {
            TaskStatus.PendingMatch => "待媒合",
            TaskStatus.PendingExecution => "待執行",
            TaskStatus.InProgress => "執行中",
            TaskStatus.UnderReview => "待驗收",
            TaskStatus.RevisionRequested => "待補件",
            TaskStatus.Completed => "已完成",
            TaskStatus.Incomplete => "已逾期",
            TaskStatus.Cancelled => "已取消",
            _ => status.ToString()
        };

    public static string ToDisplayName(this DisputeStatus status)
        => status switch
        {
            DisputeStatus.Open => "待處理",
            DisputeStatus.UnderReview => "處理中",
            DisputeStatus.ResolvedForMerchant => "維持業者",
            DisputeStatus.ResolvedForKol => "改判 KOL",
            DisputeStatus.ResolvedCompromise => "協議結案",
            DisputeStatus.Cancelled => "已取消",
            _ => status.ToString()
        };

    public static string ToPlatformDisplayName(this short platform)
        => platform switch
        {
            1 => "Instagram",
            2 => "Facebook",
            3 => "YouTube",
            4 => "TikTok",
            5 => "Blog",
            _ => "其他"
        };

    public static string ToPlatformIconClass(this short platform)
        => platform switch
        {
            1 => "fa-brands fa-instagram",
            2 => "fa-brands fa-facebook",
            3 => "fa-brands fa-youtube",
            4 => "fa-brands fa-tiktok",
            5 => "fa-solid fa-blog",
            _ => "fa-solid fa-link"
        };
}
