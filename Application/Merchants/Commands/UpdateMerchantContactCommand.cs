namespace Application.Merchants.Commands;

/// <summary>編輯業者聯絡窗口（Admin 操作）。</summary>
public sealed record UpdateMerchantContactCommand(
    long ContactId,
    long MerchantId,
    string Name,
    string? Phone,
    string? Email,
    string? Title,
    string? Note);
