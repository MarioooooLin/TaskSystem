using Domain.Enums;

namespace Application.Kols.DTOs;

/// <summary>KOL 審核列表列項 DTO（ADM-015）。</summary>
public sealed class KolReviewListItemDto
{
    public long KolId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }

    /// <summary>KOL 類型（多選）。</summary>
    public IReadOnlyList<short> Categories { get; init; } = [];

    /// <summary>主要平台（多選）。</summary>
    public IReadOnlyList<short> Platforms { get; init; } = [];

    /// <summary>粉絲總數（跨平台加總）。</summary>
    public int TotalFollowers { get; init; }

    /// <summary>資料完整度 0～100（系統動態計算）。</summary>
    public int ProfileCompleteness { get; init; }

    public VerificationStatus VerificationStatus { get; init; }

    /// <summary>送審時間（最後一次 UpdatedAt，VerificationStatus 改為 Pending 時）。</summary>
    public DateTime SubmittedAt { get; init; }
}
