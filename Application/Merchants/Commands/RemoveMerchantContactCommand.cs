namespace Application.Merchants.Commands;

/// <summary>刪除業者聯絡窗口（Admin 操作）。</summary>
public sealed record RemoveMerchantContactCommand(long ContactId, long MerchantId);
