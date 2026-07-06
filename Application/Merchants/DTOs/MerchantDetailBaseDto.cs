using Domain.Enums;

namespace Application.Merchants.DTOs;

/// <summary>
/// 業者詳情頁基本資料 DTO（由 IMerchantRepository.GetDetailBaseAsync 回傳）。
/// 子集合（聯絡窗口、統計、案件、錢包、成員、活動）由各自 Repository 另外查詢。
/// </summary>
public sealed class MerchantDetailBaseDto
{
    public long MerchantId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? EnglishName { get; init; }
    public string? TaxId { get; init; }
    public string? IndustryType { get; init; }
    public string? ContactName { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? CompanyEmail { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public DateOnly? EstablishedDate { get; init; }

    /// <summary>Owner 帳號 Email（來自 Users JOIN）。</summary>
    public string OwnerEmail { get; init; } = string.Empty;

    /// <summary>Owner 帳號名稱（來自 Users JOIN）。</summary>
    public string OwnerName { get; init; } = string.Empty;

    public VerificationStatus VerificationStatus { get; init; }
    public DateTime? VerifiedAt { get; init; }

    /// <summary>最後操作的管理員名稱（來自 Users LEFT JOIN）。</summary>
    public string? UpdatedByAdminName { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
