using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.RolePermission;

/// <summary>新增後台角色 ViewModel。</summary>
public sealed class CreateAdminRoleViewModel
{
    [Required(ErrorMessage = "請輸入角色名稱")]
    [StringLength(100, ErrorMessage = "角色名稱不可超過 100 個字元")]
    [Display(Name = "角色名稱")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "角色說明不可超過 500 個字元")]
    [Display(Name = "角色說明")]
    public string? Description { get; set; }

    [Display(Name = "系統保留")]
    public bool IsSystemReserved { get; set; }

    [Display(Name = "啟用狀態")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "權限")]
    public List<long> PermissionIds { get; set; } = [];

    public List<PermissionGroupViewModel> PermissionGroups { get; set; } = [];
}

public sealed class PermissionGroupViewModel
{
    public string GroupName { get; set; } = string.Empty;
    public List<SelectListItem> Items { get; set; } = [];
}
