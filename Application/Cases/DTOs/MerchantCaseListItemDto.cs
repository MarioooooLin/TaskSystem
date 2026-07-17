using Domain.Enums;

namespace Application.Cases.DTOs;

/// <summary>業者端案件管理列表列項 DTO。</summary>
public sealed class MerchantCaseListItemDto
{
    public long CaseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public CaseStatus Status { get; init; }

    // ── 合作條件 ──────────────────────────────────────
    public bool HasCash { get; init; }
    public bool HasBarter { get; init; }
    public bool HasCommission { get; init; }

    // ── 招募 / 任務統計 ───────────────────────────────
    public int WantedKolCount { get; init; }
    public int ApplicationCount { get; init; }
    public int ApprovedAssignmentCount { get; init; }
    public int TaskUnderReviewCount { get; init; }
    public int TaskInProgressCount { get; init; }
    public int TaskCompletedCount { get; init; }
    public int TaskIncompleteCount { get; init; }
    public int TaskCancelledCount { get; init; }

    // ── 金額 / 時間 ───────────────────────────────────
    public decimal CashRewardAmount { get; init; }
    public DateTime ApplicationDeadline { get; init; }
    public DateTime SubmissionDeadline { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    // ── 發佈平台 ──────────────────────────────────────
    public IReadOnlyList<SocialPlatform> Platforms { get; set; } = [];
}
