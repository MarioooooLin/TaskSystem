using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;
using Domain.Enums;

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
            SET    u.Status    = 2,          -- Suspended
                   u.UpdatedAt = GETUTCDATE()
            FROM   Users u
            INNER JOIN MerchantMembers mm ON mm.UserId = u.Id
            WHERE  mm.MerchantId = @MerchantId
              AND  u.Status      = 1          -- Active only
            """;

        await session.Connection.ExecuteAsync(
            sql, new { MerchantId = merchantId }, session.Transaction);
    }

    public async Task ReactivateUsersByMerchantAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE u
            SET    u.Status    = 1,          -- Active
                   u.UpdatedAt = GETUTCDATE()
            FROM   Users u
            INNER JOIN MerchantMembers mm ON mm.UserId = u.Id
            WHERE  mm.MerchantId = @MerchantId
              AND  mm.Status     = 1          -- Active member relation only
              AND  u.Status      = 2          -- Suspended only
            """;

        await session.Connection.ExecuteAsync(
            sql, new { MerchantId = merchantId }, session.Transaction);
    }

    public async Task<UserInvitation?> GetPendingInvitationByTokenAsync(
        string token, string email, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, UserId, Email, InvitedByUserId, RoleId, TokenHash,
                   AccountType, Status, ExpiresAt, AcceptedAt, CreatedAt, UpdatedAt
            FROM UserInvitations
            WHERE TokenHash = @Token
              AND Email = @Email
              AND AccountType = @AccountType
              AND Status = @Pending
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<UserInvitation>(sql, new
        {
            Token = token,
            Email = email,
            AccountType = (short)AccountType.Admin,
            Pending = (short)InvitationStatus.Pending
        }, session.Transaction);
    }

    public async Task AcceptInvitationAsync(
        long invitationId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE UserInvitations
            SET Status = @Accepted,
                AcceptedAt = @AcceptedAt,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            Id = invitationId,
            Accepted = (short)InvitationStatus.Accepted,
            AcceptedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, session.Transaction);
    }

    public async Task UpdatePasswordAsync(
        long userId, string passwordHash, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Users
            SET PasswordHash = @PasswordHash,
                UpdatedAt = @UpdatedAt
            WHERE Id = @UserId
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            PasswordHash = passwordHash,
            UpdatedAt = DateTime.UtcNow
        }, session.Transaction);
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT r.Name
            FROM UserRoles ur
            INNER JOIN Roles r ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId
              AND r.Scope = @Scope
            ORDER BY r.Name
            """;

        var names = await session.Connection.QueryAsync<string>(sql, new
        {
            UserId = userId,
            Scope = (short)RoleScope.System
        }, session.Transaction);

        return names.AsList();
    }
}
