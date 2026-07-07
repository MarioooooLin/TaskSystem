namespace Application.Merchants.Commands;

/// <summary>
/// 折扣金加值或扣回指令（Admin 高風險操作）。
/// OperationType: 1=Grant（加值）  4=Revoke（扣回）
/// </summary>
public sealed record AdjustMerchantCreditCommand(
    long MerchantId,
    short OperationType,
    decimal Amount,
    string Reason,
    string? Note,
    DateOnly? ExpiresAt);
