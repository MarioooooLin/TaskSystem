using Application.Abstractions.Persistence;
using Application.Roles.DTOs;
using Application.Roles.Queries;
using Common.Pagination;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得所有啟用中、Scope = Merchant 的角色（業者指派成員用）。</summary>
    Task<IReadOnlyList<Role>> GetActiveMerchantRolesAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 取得指定使用者的所有 Permission Code 清單。
    /// 查詢路徑：UserRoles → RolePermissions → Permissions.Code
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(long userId, IDbSession session, CancellationToken ct = default);

    /// <summary>分頁查詢 Scope = System 的後台角色列表。</summary>
    Task<(IReadOnlyList<AdminRoleListItemDto> Items, int TotalCount)> GetSystemRoleListAsync(
        string? keyword,
        bool? isActive,
        bool? isSystemReserved,
        bool? hasHighRiskPermission,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得後台角色 KPI 摘要。</summary>
    Task<AdminRoleSummaryDto> GetSystemRoleSummaryAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>依名稱與 Scope 查詢角色。</summary>
    Task<Role?> GetByNameAndScopeAsync(
        string name, RoleScope scope, IDbSession session, CancellationToken ct = default);

    /// <summary>重建角色的權限對應（先刪除後插入）。</summary>
    Task ReplacePermissionsAsync(
        long roleId, IEnumerable<long> permissionIds, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Role role, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Role role, IDbSession session, CancellationToken ct = default);
}
