namespace Application.Merchants.Commands;

/// <summary>新增業者聯絡窗口（Admin 操作）。</summary>
public sealed record AddMerchantContactCommand(
    long MerchantId,
    string Name,
    string? Phone,
    string? Email,
    string? Title,
    string? Note);
