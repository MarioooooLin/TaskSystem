using Domain.Enums;

namespace Domain.Entities;

public class Case
{
    public long Id { get; set; }
    public long MerchantId { get; set; }
    public long CreatedByUserId { get; set; }

    // 基本資料：草稿狀態允許不完整，Domain 層以 nullable 表示未填寫
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? OfficialUrl { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }

    // 時間與招募
    public int WantedKolCount { get; set; }
    public DateTime? ApplicationDeadline { get; set; }
    public DateTime? SubmissionDeadline { get; set; }

    // 報酬
    public decimal CashRewardAmount { get; set; }
    public bool IsCommissionEnabled { get; set; }
    public decimal? CommissionRate { get; set; }
    public int? CookieDays { get; set; }

    public string? DeliverableDescription { get; set; }

    // 狀態
    public CaseStatus Status { get; set; } = CaseStatus.Draft;
    public RecruitmentStatus RecruitmentStatus { get; set; } = RecruitmentStatus.NotOpen;

    // 自動執行門檻
    public decimal AutoExecutionThresholdRate { get; set; }
    public int AutoExecutionThresholdCount { get; set; }

    // 快取計數
    public int ApplicationCount { get; set; }
    public int ApprovedAssignmentCount { get; set; }

    // 時間戳
    public DateTime? PublishedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Domain 規則 ────────────────────────────────────
    public bool CanPublish() => Status == CaseStatus.Draft;
    public bool CanCancel() => Status < CaseStatus.InProgress;
    public bool CanModify() => Status < CaseStatus.InProgress;

    /// <summary>案件是否已有實際變更會影響已錄取 KOL 的權益，需重新確認。</summary>
    public bool HasSignificantChangesComparedTo(Case other)
    {
        if (other is null) return true;

        return Title != other.Title
            || Description != other.Description
            || OfficialUrl != other.OfficialUrl
            || City != other.City
            || Address != other.Address
            || WantedKolCount != other.WantedKolCount
            || ApplicationDeadline != other.ApplicationDeadline
            || SubmissionDeadline != other.SubmissionDeadline
            || CashRewardAmount != other.CashRewardAmount
            || IsCommissionEnabled != other.IsCommissionEnabled
            || CommissionRate != other.CommissionRate
            || CookieDays != other.CookieDays
            || DeliverableDescription != other.DeliverableDescription;
    }

    public void TouchUpdatedAt() => UpdatedAt = DateTime.UtcNow;
}
