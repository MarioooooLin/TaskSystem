namespace Domain.Entities;

public sealed class MerchantCreditWallet
{
    public long Id { get; set; }
    public long MerchantId { get; set; }

    /// <summary>可用折扣金餘額。</summary>
    public decimal AvailableAmount { get; set; }

    /// <summary>累計已使用折扣金。</summary>
    public decimal UsedAmount { get; set; }

    /// <summary>累計已到期折扣金。</summary>
    public decimal ExpiredAmount { get; set; }

    /// <summary>累計已扣回折扣金。</summary>
    public decimal RevokedAmount { get; set; }

    public DateTime UpdatedAt { get; set; }
}
