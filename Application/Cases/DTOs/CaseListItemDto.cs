using Domain.Enums;

namespace Application.Cases.DTOs;

/// <summary>案件監控列表列項 DTO。</summary>
public sealed class CaseListItemDto
{
    public long CaseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public long MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public CaseStatus Status { get; init; }

    // ── 合作條件 ──────────────────────────────────────
    public bool HasCash { get; init; }
    public bool HasBarter { get; init; }
    public bool HasCommission { get; init; }

    // ── 招募 / 成立 ───────────────────────────────────
    public int WantedKolCount { get; init; }
    public int ApplicationCount { get; init; }
    public int ApprovedAssignmentCount { get; init; }

    // ── 任務狀態摘要（各狀態數量）───────────────────────
    public int TaskInProgressCount { get; init; }
    public int TaskUnderReviewCount { get; init; }
    public int TaskCompletedCount { get; init; }
    public int TaskIncompleteCount { get; init; }
    public int TaskCancelledCount { get; init; }
    public int TaskDisputeCount { get; init; }

    public DateTime CreatedAt { get; init; }
}
