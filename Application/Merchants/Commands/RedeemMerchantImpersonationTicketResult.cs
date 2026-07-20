namespace Application.Merchants.Commands;

public sealed record RedeemMerchantImpersonationTicketResult(
    long TicketId,
    long MerchantId,
    string MerchantName,
    long AdminUserId,
    string AdminName);
