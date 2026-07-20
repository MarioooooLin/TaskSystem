namespace Application.Merchants.Commands;

/// <summary>
/// 建立代理登入票證後回傳的結果。PlainToken 僅此時以明文存在，不可寫入 Log。
/// </summary>
public sealed record CreateMerchantImpersonationTicketResult(
    long TicketId,
    string PlainToken,
    long MerchantId,
    string MerchantName,
    long AdminUserId,
    DateTime ExpiresAtUtc);
