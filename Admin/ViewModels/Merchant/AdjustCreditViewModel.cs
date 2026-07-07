using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Merchant;

public sealed class AdjustCreditViewModel
{
    [Required]
    public long MerchantId { get; set; }

    /// <summary>1=Grant（加值）  4=Revoke（扣回）</summary>
    [Required]
    [Range(1, 4)]
    public short OperationType { get; set; }

    [Required]
    [Range(1, 9_999_999, ErrorMessage = "金額必須大於 0。")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "異動原因為必填。")]
    [MaxLength(200)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateOnly? ExpiresAt { get; set; }
}
