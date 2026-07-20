namespace Domain.Enums;

/// <summary>
/// 業者現金錢包交易狀態。
/// 1=Pending  2=Approved  3=Rejected  4=Completed  5=Cancelled
/// </summary>
public enum MerchantWalletTransactionStatus : short
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Completed = 4,
    Cancelled = 5
}
