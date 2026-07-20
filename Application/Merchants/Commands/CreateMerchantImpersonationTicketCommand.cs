namespace Application.Merchants.Commands;

public sealed record CreateMerchantImpersonationTicketCommand(
    long MerchantId,
    string? CreatedIp = null,
    string? UserAgent = null);
