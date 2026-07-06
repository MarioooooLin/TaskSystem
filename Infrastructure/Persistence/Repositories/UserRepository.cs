using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    public async Task<Domain.Entities.User?> GetByIdAsync(
        long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, AccountType, Name, Email, PasswordHash,
                   Status, LastLoginAt, CreatedAt, UpdatedAt
            FROM Users
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Domain.Entities.User>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<Domain.Entities.User?> GetByEmailAsync(
        string email, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, AccountType, Name, Email, PasswordHash,
                   Status, LastLoginAt, CreatedAt, UpdatedAt
            FROM Users
            WHERE Email = @Email
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Domain.Entities.User>(
            sql, new { Email = email }, session.Transaction);
    }

    public async Task<long> InsertAsync(
        Domain.Entities.User user, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status, CreatedAt, UpdatedAt)
            VALUES (@AccountType, @Name, @Email, @PasswordHash, @Status, @CreatedAt, @UpdatedAt);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, new
        {
            AccountType = (short)user.AccountType,
            user.Name,
            user.Email,
            user.PasswordHash,
            Status = (short)user.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, session.Transaction);
    }

    public async Task UpdateAsync(
        Domain.Entities.User user, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Users
            SET Name          = @Name,
                Email         = @Email,
                PasswordHash  = @PasswordHash,
                Status        = @Status,
                LastLoginAt   = @LastLoginAt,
                UpdatedAt     = @UpdatedAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            user.Name,
            user.Email,
            user.PasswordHash,
            Status = (short)user.Status,
            user.LastLoginAt,
            UpdatedAt = DateTime.UtcNow,
            user.Id
        }, session.Transaction);
    }

    public async Task SuspendUsersByMerchantAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE u
            SET    u.Status    = 3,          -- Suspended
                   u.UpdatedAt = GETUTCDATE()
            FROM   Users u
            INNER JOIN MerchantMembers mm ON mm.UserId = u.Id
            WHERE  mm.MerchantId = @MerchantId
              AND  u.Status      = 1          -- Active only
            """;

        await session.Connection.ExecuteAsync(
            sql, new { MerchantId = merchantId }, session.Transaction);
    }
}
