using Application.AdminAccounts.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.AdminAccount;

/// <summary>後台帳號管理首頁 ViewModel（列表 + KPI 摘要 + 篩選條件）。</summary>
public sealed class AdminAccountIndexViewModel
{
    public PagedResult<AdminAccountListItemDto> List { get; init; } = null!;
    public AdminAccountSummaryDto Summary { get; init; } = new();
    public AdminAccountListQueryViewModel Query { get; init; } = new();
    public IReadOnlyList<AdminRoleOptionDto> AvailableRoles { get; init; } = [];
    public IReadOnlyList<AdminAccountLogItemDto> RecentLogs { get; init; } = [];
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
