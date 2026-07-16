using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Account;

public sealed class ProfileViewModel
{
    [Display(Name = "姓名")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "角色")]
    public string RolesDisplay { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入目前密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "目前密碼")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入新密碼")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "新密碼長度需在 8 到 100 個字元之間")]
    [DataType(DataType.Password)]
    [Display(Name = "新密碼")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "請再次輸入新密碼")]
    [DataType(DataType.Password)]
    [Display(Name = "確認新密碼")]
    [Compare(nameof(NewPassword), ErrorMessage = "兩次輸入的新密碼不一致")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
