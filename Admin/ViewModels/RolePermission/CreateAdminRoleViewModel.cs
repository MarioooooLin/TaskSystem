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

/// <summary>權限群組（用於 Create / Edit 頁面的權限矩陣）。</summary>
public sealed class PermissionGroupViewModel
{
    public string GroupName { get; set; } = string.Empty;
    public List<PermissionItemViewModel> Items { get; set; } = [];

    /// <summary>取得中文群組顯示名稱。</summary>
    public string GroupDisplayName => GroupName switch
    {
        "Account" => "後台帳號管理",
        "Role" => "後台角色管理",
        "RolePermission" => "角色權限設定",
        "Merchant" => "業者管理",
        "Kol" => "KOL 管理",
        "CaseMonitor" => "案件監控",
        "Dispute" => "爭議處理",
        "Finance" => "帳務管理",
        "Payout" => "款項撥付",
        "SystemSettings" => "系統參數設定",
        "Dashboard" => "營運總覽 Dashboard",
        _ => GroupName
    };

    /// <summary>取得群組說明。</summary>
    public string GroupDescription => GroupName switch
    {
        "Account" => "後台帳號管理相關權限",
        "Role" => "後台角色管理相關權限",
        "RolePermission" => "角色權限設定相關權限",
        "Merchant" => "業者管理相關權限",
        "Kol" => "KOL 管理相關權限",
        "CaseMonitor" => "案件監控相關權限",
        "Dispute" => "爭議處理相關權限",
        "Finance" => "帳務管理相關權限",
        "Payout" => "款項撥付相關權限",
        "SystemSettings" => "系統參數設定相關權限",
        "Dashboard" => "營運總覽 Dashboard 相關權限",
        _ => "系統功能權限"
    };
}

/// <summary>單一權限項目（用於 Create / Edit 頁面的權限矩陣）。</summary>
public sealed class PermissionItemViewModel
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short RiskLevel { get; set; }

    /// <summary>取得權限動作名稱（Code 最後一段）。</summary>
    public string ActionName
    {
        get
        {
            var lastDot = Code.LastIndexOf('.');
            return lastDot >= 0 && lastDot < Code.Length - 1
                ? Code[(lastDot + 1)..]
                : Code;
        }
    }

    /// <summary>取得中文動作顯示名稱。</summary>
    public string ActionDisplayName => ActionName switch
    {
        "View" => "檢視",
        "Edit" => "編輯",
        "Update" => "更新",
        "Manage" => "管理",
        "Review" => "審核",
        "Suspend" => "停用",
        "Resolve" => "結案",
        "Handle" => "處理",
        "Approve" => "核准",
        "ChangeStatus" => "變更狀態",
        "CreditAdjust" => "信用額度調整",
        "Impersonate" => "代理登入",
        _ => ActionName
    };
}
