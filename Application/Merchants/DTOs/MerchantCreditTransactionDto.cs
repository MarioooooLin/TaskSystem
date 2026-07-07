namespace Application.Merchants.DTOs;

/// <summary>業者折扣金交易紀錄列項（詳情頁顯示用）。</summary>
public sealed class MerchantCreditTransactionDto
{
    public long Id { get; init; }

    /// <summary>1=Grant  2=Use  3=Refund  4=Revoke  5=Expire  6=ManualAdjustment</summary>
    public short Type { get; init; }

    public decimal Amount { get; init; }
    public long? RelatedCaseId { get; init; }
    public string? Reason { get; init; }
    public string? Note { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
}
