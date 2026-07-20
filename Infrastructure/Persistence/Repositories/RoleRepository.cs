using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Roles.DTOs;
using Application.Roles.Queries;
using Common.Pagination;
using Dapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    public async Task<Role?> GetByIdAsync(
        long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Scope, IsActive, CreatedAt, UpdatedAt
            FROM Roles
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Role>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<IReadOnlyList<Role>> GetActiveMerchantRolesAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Scope, IsActive, CreatedAt, UpdatedAt
            FROM Roles
            WHERE Scope = @Scope
              AND IsActive = 1
            ORDER BY Name
            """;

        var result = await session.Connection.QueryAsync<Role>(
            sql, new { Scope = (short)RoleScope.Merchant }, session.Transaction);

        return result.AsList();
    }

    /// <summary>
    /// 取得指定使用者的所有 Permission Code。
    /// 查詢路徑：UserRoles → RolePermissions → Permissions
    /// 以及 MerchantMembers → RolePermissions → Permissions
    /// </summary>
    public async Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT p.Code
            FROM (
                SELECT ur.RoleId
                FROM UserRoles ur
                WHERE ur.UserId = @UserId
                UNION
                SELECT mm.RoleId
                FROM MerchantMembers mm
                WHERE mm.UserId = @UserId
                  AND mm.Status = 1
            ) roles
            INNER JOIN RolePermissions rp ON rp.RoleId = roles.RoleId
            INNER JOIN Permissions p      ON p.Id = rp.PermissionId
            """;

        var result = await session.Connection.QueryAsync<string>(
            sql, new { UserId = userId }, session.Transaction);

        return result.AsList();
    }

    public async Task<Role?> GetByNameAndScopeAsync(
        string name, RoleScope scope, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Scope, IsSystemReserved, IsActive, CreatedAt, UpdatedAt
            FROM Roles
            WHERE Name = @Name AND Scope = @Scope
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Role>(sql, new
        {
            Name = name,
            Scope = (short)scope
        }, session.Transaction);
    }

    public async Task ReplacePermissionsAsync(
        long roleId, IEnumerable<long> permissionIds, IDbSession session, CancellationToken ct = default)
    {
        const string deleteSql = """
            DELETE FROM RolePermissions WHERE RoleId = @RoleId
            """;

        const string insertSql = """
            INSERT INTO RolePermissions (RoleId, PermissionId)
            VALUES (@RoleId, @PermissionId)
            """;

        await session.Connection.ExecuteAsync(deleteSql, new { RoleId = roleId }, session.Transaction);

        var distinctIds = permissionIds.Distinct().ToList();
        if (distinctIds.Count > 0)
        {
            await session.Connection.ExecuteAsync(
                insertSql,
                distinctIds.Select(pid => new { RoleId = roleId, PermissionId = pid }),
                session.Transaction);
        }
    }

    public async Task<long> InsertAsync(
        Role role, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Roles (Name, Description, Scope, IsSystemReserved, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Description, @Scope, @IsSystemReserved, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, new
        {
            role.Name,
            role.Description,
            Scope = (short)role.Scope,
            role.IsSystemReserved,
            role.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, session.Transaction);
    }

    public async Task UpdateAsync(
        Role role, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Roles
            SET Name             = @Name,
                Description      = @Description,
                Scope            = @Scope,
                IsSystemReserved = @IsSystemReserved,
                IsActive         = @IsActive,
                UpdatedAt        = @UpdatedAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            role.Name,
            role.Description,
            Scope = (short)role.Scope,
            role.IsSystemReserved,
            role.IsActive,
            UpdatedAt = DateTime.UtcNow,
            role.Id
        }, session.Transaction);
    }

    /// <summary>
    /// 分頁查詢 Scope = System 的後台角色列表，包含使用帳號數與高風險權限標記。
    /// </summary>
    public async Task<(IReadOnlyList<AdminRoleListItemDto> Items, int TotalCount)> GetSystemRoleListAsync(
        string? keyword,
        bool? isActive,
        bool? isSystemReserved,
        bool? hasHighRiskPermission,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string conditions = """
            r.Scope = @Scope
            AND (@Keyword IS NULL OR r.Name LIKE '%' + @Keyword + '%' OR r.Description LIKE '%' + @Keyword + '%')
            AND (@IsActive IS NULL OR r.IsActive = @IsActive)
            AND (@IsSystemReserved IS NULL OR r.IsSystemReserved = @IsSystemReserved)
            AND (@HasHighRiskPermission IS NULL OR
                (@HasHighRiskPermission = 1 AND EXISTS (
                    SELECT 1 FROM RolePermissions rp2
                    INNER JOIN Permissions p2 ON p2.Id = rp2.PermissionId
                    WHERE rp2.RoleId = r.Id AND p2.RiskLevel = 2
                )) OR
                (@HasHighRiskPermission = 0 AND NOT EXISTS (
                    SELECT 1 FROM RolePermissions rp2
                    INNER JOIN Permissions p2 ON p2.Id = rp2.PermissionId
                    WHERE rp2.RoleId = r.Id AND p2.RiskLevel = 2
                )))
            """;

        var listSql = $"""
            SELECT r.Id, r.Name, r.Description, r.IsActive, r.IsSystemReserved, r.UpdatedAt,
                (SELECT COUNT(*) FROM UserRoles ur WHERE ur.RoleId = r.Id) AS UsageCount,
                CASE WHEN EXISTS (
                    SELECT 1 FROM RolePermissions rp
                    INNER JOIN Permissions p ON p.Id = rp.PermissionId
                    WHERE rp.RoleId = r.Id AND p.RiskLevel = 2
                ) THEN 1 ELSE 0 END AS HasHighRiskPermission
            FROM Roles r
            WHERE {conditions}
            ORDER BY r.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var countSql = $"""
            SELECT COUNT(*)
            FROM Roles r
            WHERE {conditions}
            """;

        var param = new
        {
            Scope = (short)RoleScope.System,
            Keyword = keyword,
            IsActive = isActive,
            IsSystemReserved = isSystemReserved,
            HasHighRiskPermission = hasHighRiskPermission,
            Offset = page.Offset,
            PageSize = page.PageSize
        };

        var items = await session.Connection.QueryAsync<AdminRoleListItemDto>(
            listSql, param, session.Transaction);

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        return (items.AsList(), totalCount);
    }

    /// <summary>
    /// 取得後台角色 KPI 摘要。
    /// </summary>
    public async Task<AdminRoleSummaryDto> GetSystemRoleSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM Roles WHERE Scope = @Scope) AS TotalCount,
                (SELECT COUNT(*) FROM Roles WHERE Scope = @Scope AND IsActive = 1) AS ActiveCount,
                (SELECT COUNT(*) FROM Roles WHERE Scope = @Scope AND IsActive = 0) AS DisabledCount,
                (SELECT COUNT(DISTINCT ur.UserId)
                 FROM UserRoles ur
                 INNER JOIN Roles r ON r.Id = ur.RoleId
                 WHERE r.Scope = @Scope) AS InUseAccountCount,
                (SELECT COUNT(DISTINCT r.Id)
                 FROM Roles r
                 WHERE r.Scope = @Scope
                   AND EXISTS (
                       SELECT 1 FROM RolePermissions rp
                       INNER JOIN Permissions p ON p.Id = rp.PermissionId
                       WHERE rp.RoleId = r.Id AND p.RiskLevel = 2
                   )) AS HighRiskRoleCount
            """;

        return await session.Connection.QuerySingleAsync<AdminRoleSummaryDto>(
            sql, new { Scope = (short)RoleScope.System }, session.Transaction);
    }
}
