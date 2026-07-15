using Application.Abstractions.Persistence;
using Application.Roles.DTOs;

namespace Application.Abstractions.Repositories;

public interface IPermissionRepository
{
    /// <summary>確認給定的 Permission Id 都存在，回傳存在的 Id 集合。</summary>
    Task<IReadOnlyList<long>> GetExistingIdsAsync(
        IEnumerable<long> ids,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得所有後台權限項目，依群組與 Code 排序。</summary>
    Task<IReadOnlyList<AdminPermissionDto>> GetAllSystemPermissionsAsync(
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得指定角色已擁有的 Permission Id 清單。</summary>
    Task<IReadOnlyList<long>> GetPermissionIdsByRoleIdAsync(
        long roleId,
        IDbSession session,
        CancellationToken ct = default);
}
