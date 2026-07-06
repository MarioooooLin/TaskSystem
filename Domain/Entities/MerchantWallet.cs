namespace Domain.Entities;

public class MerchantWallet
{
    public long Id { get; set; }
    public long MerchantId { get; set; }

    public decimal AvailableAmount { get; set; }
    public decimal FrozenAmount { get; set; }
    public decimal TotalDepositedAmount { get; set; }

    public DateTime UpdatedAt { get; set; }

    // ── Domain 規則（待實作）────────────────────────────
    public bool HasSufficientBalance(decimal amount)
        => AvailableAmount >= amount;

    public decimal TotalAmount => AvailableAmount + FrozenAmount;
}
