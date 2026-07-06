using Domain.Enums;

namespace Application.Kols.DTOs;

/// <summary>KOL 列表列項 DTO（ADM-005）。</summary>
public sealed class KolListItemDto
{
    public long KolId { get; init; }
    public long UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }

    /// <summary>KOL 類型（多選，逗號分隔的 SMALLINT 清單，由 Repository 組裝）。</summary>
    public IReadOnlyList<short> Categories { get; init; } = [];

    /// <summary>主要平台（多選）。</summary>
    public IReadOnlyList<short> Platforms { get; init; } = [];

    /// <summary>粉絲總數（跨平台加總）。</summary>
    public int TotalFollowers { get; init; }

    public VerificationStatus VerificationStatus { get; init; }

    /// <summary>收款資料狀態：1=Pending 2=Verified 3=Rejected，NULL=未填。</summary>
    public short? BankAccountStatus { get; init; }

    public int TaskCount { get; init; }
    public int DisputeCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
