using Domain.Enums;

namespace Application.Merchants.DTOs;

/// <summary>業者列表列項 DTO（Admin 後台用）。</summary>
public sealed class MerchantListItemDto
{
    public long MerchantId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? TaxId { get; init; }
    public string? IndustryType { get; init; }
    public string? ContactName { get; init; }
    public string? Phone { get; init; }

    /// <summary>業者 Owner 的 Email（來自 Users 表）。</summary>
    public string OwnerEmail { get; init; } = string.Empty;

    public VerificationStatus VerificationStatus { get; init; }
    public DateTime CreatedAt { get; init; }

    /// <summary>錢包可用金額（快覽用）。</summary>
    public decimal AvailableAmount { get; init; }

    /// <summary>折扣金餘額（快覽用）。</summary>
    public decimal CreditAmount { get; init; }

    /// <summary>案件總數（快覽用）。</summary>
    public int CaseCount { get; init; }
}
