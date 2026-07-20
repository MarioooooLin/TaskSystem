using Domain.Enums;

namespace Application.Cases.DTOs;

/// <summary>案件編輯頁載入時的完整資料 DTO。</summary>
public sealed class CaseEditDto
{
    public long CaseId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }

    // ── 地點 ──────────────────────────────────────────
    public int? CityId { get; init; }
    public string? Address { get; init; }
    public string? OfficialUrl { get; init; }

    // ── 分類 / 語言 / 平台 ─────────────────────────────
    public IReadOnlyList<int> Categories { get; init; } = [];
    public IReadOnlyList<string> Languages { get; init; } = [];
    public IReadOnlyList<short> Platforms { get; init; } = [];

    // ── 報酬 ──────────────────────────────────────────
    public bool HasCash { get; init; }
    public decimal? CashRewardAmount { get; init; }
    public bool HasCommission { get; init; }
    public decimal? CommissionRate { get; init; }
    public int? CookieDays { get; init; }

    // ── 時程 ──────────────────────────────────────────
    public DateTime? ApplicationDeadline { get; init; }
    public DateTime? SubmissionDeadline { get; init; }

    // ── 招募 ──────────────────────────────────────────
    public int WantedKolCount { get; init; }

    // ── 任務交付物 ────────────────────────────────────
    public string? DeliverableDescription { get; init; }

    // ── 門檻與條件 ────────────────────────────────────
    public int? MinFollowers { get; init; }
    public string? RequirementNotes { get; init; }

    // ── 以物易物 ──────────────────────────────────────
    public IReadOnlyList<CaseBarterItemDto> BarterItems { get; init; } = [];

    // ── 附件 ──────────────────────────────────────────
    public IReadOnlyList<CaseAttachmentDto> Attachments { get; init; } = [];

    // ── 狀態（編輯時用於判斷是否已發布）───────────────
    public CaseStatus Status { get; init; }
}

public sealed class CaseBarterItemDto
{
    public long? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int? Quantity { get; init; }
    public string? Note { get; init; }
}
