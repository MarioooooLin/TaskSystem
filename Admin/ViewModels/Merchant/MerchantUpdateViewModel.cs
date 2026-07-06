using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Merchant;

/// <summary>編輯業者基本資料 ViewModel。</summary>
public sealed class MerchantUpdateViewModel
{
    public long MerchantId { get; set; }

    [Required(ErrorMessage = "公司名稱為必填。")]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? EnglishName { get; set; }

    [MaxLength(20)]
    public string? TaxId { get; set; }

    [MaxLength(100)]
    public string? IndustryType { get; set; }

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Fax { get; set; }

    [MaxLength(255)]
    [EmailAddress(ErrorMessage = "公司 Email 格式不正確。")]
    public string? CompanyEmail { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "網站 URL 格式不正確。")]
    public string? Website { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public DateOnly? EstablishedDate { get; set; }
}
