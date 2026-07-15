using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Roles.DTOs;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class PermissionRepository : IPermissionRepository
{
    public async Task<IReadOnlyList<long>> GetExistingIdsAsync(
        IEnumerable<long> ids, IDbSession session, CancellationToken ct = default)
    {
        var distinctIds = ids.Distinct().ToList();
        if (distinctIds.Count == 0)
            return Array.Empty<long>();

        const string sql = """
            SELECT Id FROM Permissions WHERE Id IN @Ids
            """;

        var result = await session.Connection.QueryAsync<long>(
            sql, new { Ids = distinctIds }, session.Transaction);
        return result.AsList();
    }

    public async Task<IReadOnlyList<AdminPermissionDto>> GetAllSystemPermissionsAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                Id,
                Code,
                Description,
                RiskLevel,
                CASE
                    WHEN LEN(Code) - LEN(REPLACE(Code, '.', '')) >= 2
                        THEN PARSENAME(REPLACE(Code, '.', '.'), 2)
                    WHEN CHARINDEX('.', Code) > 0
                        THEN PARSENAME(REPLACE(Code, '.', '.'), 1)
                    ELSE Code
                END AS GroupName
            FROM Permissions
            ORDER BY GroupName, Code
            """;

        var result = await session.Connection.QueryAsync<AdminPermissionDto>(
            sql, transaction: session.Transaction);
        return result.AsList();
    }

    public async Task<IReadOnlyList<long>> GetPermissionIdsByRoleIdAsync(
        long roleId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT PermissionId
            FROM RolePermissions
            WHERE RoleId = @RoleId
            """;

        var result = await session.Connection.QueryAsync<long>(
            sql, new { RoleId = roleId }, session.Transaction);
        return result.AsList();
    }
}
