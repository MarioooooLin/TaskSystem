using Domain.Enums;

namespace Admin.ViewModels.AdminAccount;

/// <summary>後台帳號列表頁搜尋條件 ViewModel。</summary>
public sealed class AdminAccountListQueryViewModel
{
    public string? Keyword { get; set; }
    public UserStatus? Status { get; set; }
    public string? Department { get; set; }
    public long? RoleId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
