namespace Application.Cases.DTOs;

/// <summary>案件發布確認頁預覽資料。</summary>
public sealed class PublishPreviewDto
{
    public long CaseId { get; init; }
    public string Title { get; init; } = string.Empty;

    // ── 地點 ──────────────────────────────────────────
    public string? City { get; init; }
    public string? Address { get; init; }

    // ── 招募與報酬 ────────────────────────────────────
    public int WantedKolCount { get; init; }
    public decimal RewardAmountPerKol { get; init; }
    public decimal RewardSubtotal { get; init; }

    // ── 合作條件 ──────────────────────────────────────
    public bool HasCommission { get; init; }
    public decimal? CommissionRate { get; init; }
    public IReadOnlyList<PublishPreviewBarterItemDto> BarterItems { get; init; } = [];

    // ── 平台與交付 ────────────────────────────────────
    public IReadOnlyList<short> Platforms { get; init; } = [];
    public string? DeliverableDescription { get; init; }

    // ── 附件 ──────────────────────────────────────────
    public IReadOnlyList<PublishPreviewAttachmentDto> Attachments { get; init; } = [];

    // ── 費用明細 ──────────────────────────────────────
    public IReadOnlyList<PublishPreviewFeeItemDto> FeeItems { get; init; } = [];
    public decimal CaseOpeningFee { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal PlatformServiceFee { get; init; }
    public decimal EstimatedFrozenAmount { get; init; }

    // ── 目前錢包餘額 ──────────────────────────────────
    public decimal CurrentWalletBalance { get; init; }
    public bool HasEnoughBalance { get; init; }

    // ── 發布後狀態預覽 ────────────────────────────────
    public DateTime ApplicationDeadline { get; init; }
    public DateTime SubmissionDeadline { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
}

public sealed class PublishPreviewFeeItemDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Note { get; init; }
}

public sealed class PublishPreviewBarterItemDto
{
    public string Name { get; init; } = string.Empty;
    public int? Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed class PublishPreviewAttachmentDto
{
    public long Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public int FileSize { get; init; }
    public string MimeType { get; init; } = string.Empty;
}
