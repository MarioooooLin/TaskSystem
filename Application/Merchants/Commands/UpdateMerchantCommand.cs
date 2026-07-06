namespace Application.Merchants.Commands;

/// <summary>編輯業者基本資料（Admin 操作）。</summary>
public sealed record UpdateMerchantCommand(
    long MerchantId,
    string CompanyName,
    string? EnglishName,
    string? TaxId,
    string? IndustryType,
    string? ContactName,
    string? Phone,
    string? Fax,
    string? CompanyEmail,
    string? Website,
    string? Address,
    DateOnly? EstablishedDate);
