using Domain.Enums;

namespace Domain.Entities;

public class Case
{
    public long Id { get; set; }
    public long MerchantId { get; set; }
    public long CreatedByUserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OfficialUrl { get; set; }
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public int WantedKolCount { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public DateTime SubmissionDeadline { get; set; }

    // 報酬
    public decimal CashRewardAmount { get; set; }
    public bool IsCommissionEnabled { get; set; }
    public decimal? CommissionRate { get; set; }
    public int? CookieDays { get; set; }

    public string DeliverableDescription { get; set; } = string.Empty;

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

    // ── Domain 規則（待實作）────────────────────────────
    public bool CanPublish() => Status == CaseStatus.Draft;
    public bool CanCancel() => Status < CaseStatus.InProgress;
    public bool CanModify() => Status < CaseStatus.InProgress;
}
