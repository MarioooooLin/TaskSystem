using Domain.Enums;

namespace Application.Cases.DTOs;

/// <summary>案件詳情完整 DTO（含任務清單、附件、操作紀錄）。</summary>
public sealed class CaseDetailDto
{
    public long CaseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public CaseStatus Status { get; init; }
    public long MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;

    // ── 合作條件 ────────────────────────────────────────
    public bool HasCash { get; init; }
    public bool HasBarter { get; init; }
    public bool HasCommission { get; init; }
    public decimal CashRewardAmount { get; init; }
    public IReadOnlyList<string> BarterItems { get; init; } = [];
    public decimal? CommissionRate { get; init; }
    public int? CookieDays { get; init; }

    // ── 時程 ────────────────────────────────────────────
    public DateTime ApplicationDeadline { get; init; }
    public DateTime SubmissionDeadline { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }

    // ── 招募統計 ─────────────────────────────────────────
    public int WantedKolCount { get; init; }
    public int ApplicationCount { get; init; }
    public int ApprovedAssignmentCount { get; init; }
    public int RejectedApplicationCount { get; init; }

    // ── 任務統計 ─────────────────────────────────────────
    public int TaskInProgressCount { get; init; }
    public int TaskUnderReviewCount { get; init; }
    public int TaskRevisionCount { get; init; }
    public int TaskCompletedCount { get; init; }
    public int TaskIncompleteCount { get; init; }
    public int TaskCancelledCount { get; init; }
    public int TaskDisputeCount { get; init; }

    // ── 建立資訊 ─────────────────────────────────────────
    public string? CreatedByName { get; init; }
    public IReadOnlyList<short> Platforms { get; init; } = [];
    public DateTime CreatedAt { get; init; }

    // ── 子清單 ───────────────────────────────────────────
    public IReadOnlyList<CaseTaskListItemDto> Tasks { get; init; } = [];
    public IReadOnlyList<CaseAttachmentDto> Attachments { get; init; } = [];
    public IReadOnlyList<CaseActivityLogDto> ActivityLogs { get; init; } = [];
}

/// <summary>KOL 任務清單列項 DTO。</summary>
public sealed class CaseTaskListItemDto
{
    public long TaskId { get; init; }
    public long? KolId { get; init; }
    public string KolName { get; init; } = string.Empty;
    public short? KolMainPlatform { get; init; }
    public short? KolFirstCategory { get; init; }
    public Domain.Enums.TaskStatus TaskStatus { get; init; }
    public SubmissionStatus? SubmissionStatus { get; init; }
    public string? SubmissionUrl { get; init; }
    public bool HasDispute { get; init; }
    public DisputeStatus? DisputeStatus { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>案件附件 DTO。</summary>
public sealed class CaseAttachmentDto
{
    public long Id { get; init; }
    public long FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public short AttachmentType { get; init; }
}

/// <summary>案件操作紀錄 DTO。</summary>
public sealed class CaseActivityLogDto
{
    public long Id { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? Note { get; init; }
    public string? ActorName { get; init; }
    public DateTime CreatedAt { get; init; }
}
