using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.AdminAccount;

/// <summary>編輯後台帳號 ViewModel。</summary>
public sealed class EditAdminAccountViewModel
{
    public long UserId { get; set; }

    [Required(ErrorMessage = "請輸入姓名")]
    [StringLength(100, ErrorMessage = "姓名不可超過 100 個字元")]
    [Display(Name = "姓名")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入電子郵件")]
    [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
    [StringLength(255, ErrorMessage = "電子郵件不可超過 255 個字元")]
    [Display(Name = "電子郵件")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "請至少選擇一個角色")]
    [MinLength(1, ErrorMessage = "請至少選擇一個角色")]
    [Display(Name = "角色")]
    public long[] RoleIds { get; set; } = [];

    [StringLength(100, ErrorMessage = "部門不可超過 100 個字元")]
    [Display(Name = "部門")]
    public string? Department { get; set; }

    [StringLength(100, ErrorMessage = "職稱不可超過 100 個字元")]
    [Display(Name = "職稱")]
    public string? JobTitle { get; set; }

    [StringLength(50, ErrorMessage = "聯絡電話不可超過 50 個字元")]
    [Display(Name = "聯絡電話")]
    public string? Phone { get; set; }

    [StringLength(500, ErrorMessage = "備註不可超過 500 個字元")]
    [Display(Name = "備註")]
    public string? Note { get; set; }

    [Display(Name = "帳號狀態")]
    public UserStatus Status { get; set; }

    public List<SelectListItem> AvailableRoles { get; set; } = [];
}
