using System.ComponentModel.DataAnnotations;

namespace Merchant.ViewModels.Account;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "請輸入公司統一編號")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "統一編號須為 8 位數字")]
    [StringLength(8)]
    [Display(Name = "公司統一編號")]
    public string TaxId { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入電子郵件")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    [StringLength(255)]
    [Display(Name = "帳號 / 電子郵件")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "密碼長度需在 8 到 100 個字元之間")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "記住我")]
    public bool RememberMe { get; set; }
}
