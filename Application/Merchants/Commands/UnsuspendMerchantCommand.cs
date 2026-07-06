namespace Application.Merchants.Commands;

/// <summary>解除停用業者（Admin 操作）。</summary>
public sealed record UnsuspendMerchantCommand(long MerchantId);
