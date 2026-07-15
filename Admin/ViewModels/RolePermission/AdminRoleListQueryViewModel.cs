namespace Admin.ViewModels.RolePermission;

/// <summary>後台角色管理列表頁搜尋條件 ViewModel。</summary>
public sealed class AdminRoleListQueryViewModel
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSystemReserved { get; set; }
    public bool? HasHighRiskPermission { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
