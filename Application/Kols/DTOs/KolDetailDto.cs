using Domain.Enums;

namespace Application.Kols.DTOs;

/// <summary>KOL 詳情頁完整聚合 DTO（ADM-006 / ADM-016）。</summary>
public sealed class KolDetailDto
{
    // ── 基本資料 ──────────────────────────────────────────
    public long KolId { get; init; }
    public long UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? RealName { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? LineContactId { get; init; }
    public bool AcceptsCash { get; init; }
    public bool AcceptsBarter { get; init; }
    public bool AcceptsCommission { get; init; }
    public string? Intro { get; init; }
    public IReadOnlyList<short> Categories { get; init; } = [];
    public DateTime CreatedAt { get; init; }

    // ── KOL 狀態 ──────────────────────────────────────────
    public VerificationStatus VerificationStatus { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public string? VerifiedByAdminName { get; init; }
    public string? RejectionNote { get; init; }
    public string? SuspensionNote { get; init; }

    /// <summary>最近一次 KolProfiles.UpdatedAt，即送審時間的近似值。</summary>
    public DateTime SubmittedAt { get; init; }

    // ── 統計數字 ──────────────────────────────────────────
    public int TotalFollowers { get; init; }
    public int TaskCount { get; init; }
    public int CompletedTaskCount { get; init; }
    public int PendingReviewCount { get; init; }
    public int DisputeCount { get; init; }

    // ── 社群帳號 ──────────────────────────────────────────
    public IReadOnlyList<KolSocialAccountDto> SocialAccounts { get; init; } = [];

    // ── 收款資料 ──────────────────────────────────────────
    public KolBankAccountDto? BankAccount { get; init; }

    // ── 近期任務（最新 10 筆） ────────────────────────────
    public IReadOnlyList<KolTaskSummaryDto> RecentTasks { get; init; } = [];

    // ── 收益概況 ──────────────────────────────────────────
    public KolEarningsSummaryDto Earnings { get; init; } = new();

    // ── 近期活動紀錄（最新 10 筆） ───────────────────────
    public IReadOnlyList<KolActivityLogDto> RecentActivityLogs { get; init; } = [];
}
