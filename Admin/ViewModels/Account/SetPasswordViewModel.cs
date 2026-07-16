using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Account;

public sealed class SetPasswordViewModel
{
    public string Token { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "姓名")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入密碼")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "密碼長度需在 8 到 100 個字元之間")]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "請再次輸入密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認密碼")]
    [Compare(nameof(Password), ErrorMessage = "兩次輸入的密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
