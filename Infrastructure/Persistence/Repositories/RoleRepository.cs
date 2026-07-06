using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
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
    /// </summary>
    public async Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT DISTINCT p.Code
            FROM UserRoles ur
            INNER JOIN RolePermissions rp ON rp.RoleId = ur.RoleId
            INNER JOIN Permissions p      ON p.Id = rp.PermissionId
            WHERE ur.UserId = @UserId
            """;

        var result = await session.Connection.QueryAsync<string>(
            sql, new { UserId = userId }, session.Transaction);

        return result.AsList();
    }

    public async Task<long> InsertAsync(
        Role role, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Roles (Name, Scope, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Name, @Scope, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, new
        {
            role.Name,
            Scope = (short)role.Scope,
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
            SET Name      = @Name,
                Scope     = @Scope,
                IsActive  = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            role.Name,
            Scope = (short)role.Scope,
            role.IsActive,
            UpdatedAt = DateTime.UtcNow,
            role.Id
        }, session.Transaction);
    }
}
