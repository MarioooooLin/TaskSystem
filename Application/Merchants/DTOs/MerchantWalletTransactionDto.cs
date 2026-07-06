namespace Application.Merchants.DTOs;

/// <summary>業者錢包交易紀錄列項（詳情頁交易明細）。</summary>
public sealed class MerchantWalletTransactionDto
{
    public long Id { get; init; }

    /// <summary>1=OfflineDeposit 2=TaskBudgetFreeze 3=TaskBudgetRelease 4=TaskBudgetSettle 5=DisputeHold 6=ManualAdjustment</summary>
    public short Type { get; init; }

    public decimal Amount { get; init; }

    /// <summary>1=Pending 2=Approved 3=Rejected 4=Completed 5=Cancelled</summary>
    public short Status { get; init; }

    public long? RelatedCaseId { get; init; }
    public string? RelatedCaseTitle { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
