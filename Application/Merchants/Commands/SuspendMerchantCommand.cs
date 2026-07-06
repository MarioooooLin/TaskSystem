namespace Application.Merchants.Commands;

/// <summary>停用業者（Admin 操作）。</summary>
public sealed record SuspendMerchantCommand(long MerchantId);
