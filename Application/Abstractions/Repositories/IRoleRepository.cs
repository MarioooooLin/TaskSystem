using Application.Abstractions.Persistence;
using Domain.Entities;

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

    Task<long> InsertAsync(Role role, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Role role, IDbSession session, CancellationToken ct = default);
}
