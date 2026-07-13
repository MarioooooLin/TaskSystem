using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.AdminAccounts.DTOs;
using Common.Pagination;
using Dapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class AdminAccountRepository : IAdminAccountRepository
{
    public async Task<(IReadOnlyList<AdminAccountListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        UserStatus? status,
        string? department,
        long? roleId,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default)
    {
        var where = new List<string> { "u.AccountType = @AccountType" };
        if (!string.IsNullOrWhiteSpace(keyword))
            where.Add("(u.Name LIKE @Keyword OR u.Email LIKE @Keyword OR ap.Department LIKE @Keyword)");
        if (status.HasValue)
            where.Add("u.Status = @Status");
        if (!string.IsNullOrWhiteSpace(department))
            where.Add("ap.Department = @Department");
        if (roleId.HasValue)
            where.Add("EXISTS (SELECT 1 FROM UserRoles ur2 WHERE ur2.UserId = u.Id AND ur2.RoleId = @RoleId)");

        var whereSql = "WHERE " + string.Join(" AND ", where);

        var countSql = $"""
            SELECT COUNT(DISTINCT u.Id)
            FROM Users u
            LEFT JOIN AdminProfiles ap ON ap.UserId = u.Id
            {whereSql}
            """;

        var dataSql = $"""
            SELECT
                u.Id              AS UserId,
                u.Name            AS Name,
                u.Email           AS Email,
                ap.Department     AS Department,
                (
                    SELECT STRING_AGG(r.Name, ', ')
                    FROM UserRoles ur
                    INNER JOIN Roles r ON r.Id = ur.RoleId
                    WHERE ur.UserId = u.Id
                )                 AS RolesDisplay,
                u.Status          AS Status,
                CAST(CASE
                    WHEN EXISTS (
                        SELECT 1 FROM UserInvitations ui
                        WHERE ui.UserId = u.Id
                          AND ui.AccountType = @AccountType
                          AND ui.Status = @InvitationPending
                    ) THEN 1 ELSE 0
                END AS BIT)       AS HasPendingInvitation,
                CAST(CASE
                    WHEN EXISTS (
                        SELECT 1 FROM UserInvitations ui
                        WHERE ui.UserId = u.Id
                          AND ui.AccountType = @AccountType
                          AND ui.Status = @InvitationPending
                          AND ui.ExpiresAt < GETUTCDATE()
                    ) THEN 1 ELSE 0
                END AS BIT)       AS IsInvitationExpired,
                u.LastLoginAt     AS LastLoginAt,
                u.CreatedAt       AS CreatedAt
            FROM Users u
            LEFT JOIN AdminProfiles ap ON ap.UserId = u.Id
            {whereSql}
            ORDER BY u.CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            AccountType = (short)AccountType.Admin,
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = status.HasValue ? (short)status.Value : (short?)null,
            Department = string.IsNullOrWhiteSpace(department) ? null : department.Trim(),
            RoleId = roleId,
            InvitationPending = (short)InvitationStatus.Pending,
            Offset = page.Offset,
            PageSize = page.PageSize
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        var items = await session.Connection.QueryAsync<AdminAccountListItemDto>(
            dataSql, param, session.Transaction);

        return (items.AsList(), totalCount);
    }

    public async Task<AdminAccountSummaryDto> GetSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*) AS TotalCount,
                SUM(CASE WHEN u.Status = @Active THEN 1 ELSE 0 END) AS ActiveCount,
                SUM(CASE WHEN u.Status = @Suspended THEN 1 ELSE 0 END) AS SuspendedCount
            FROM Users u
            WHERE u.AccountType = @AccountType
              AND u.Status <> @Deleted
            """;

        var baseSummary = await session.Connection.QueryFirstAsync<AdminAccountSummaryDto>(sql, new
        {
            AccountType = (short)AccountType.Admin,
            Active = (short)UserStatus.Active,
            Suspended = (short)UserStatus.Suspended,
            Deleted = (short)UserStatus.Deleted
        }, session.Transaction);

        const string invitationSql = """
            SELECT
                SUM(CASE WHEN Status = @Pending THEN 1 ELSE 0 END) AS PendingInvitationCount,
                SUM(CASE WHEN Status = @Pending AND ExpiresAt < GETUTCDATE() THEN 1 ELSE 0 END) AS ExpiredInvitationCount
            FROM UserInvitations
            WHERE AccountType = @AccountType
            """;

        var invitationSummary = await session.Connection.QueryFirstAsync<AdminAccountSummaryDto>(invitationSql, new
        {
            AccountType = (short)AccountType.Admin,
            Pending = (short)InvitationStatus.Pending
        }, session.Transaction);

        return new AdminAccountSummaryDto
        {
            TotalCount = baseSummary.TotalCount,
            ActiveCount = baseSummary.ActiveCount,
            PendingInvitationCount = invitationSummary.PendingInvitationCount,
            SuspendedCount = baseSummary.SuspendedCount,
            ExpiredInvitationCount = invitationSummary.ExpiredInvitationCount
        };
    }

    public async Task<AdminAccountEditDto?> GetByIdAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                u.Id          AS UserId,
                u.Name        AS Name,
                u.Email       AS Email,
                ap.Department AS Department,
                ap.JobTitle   AS JobTitle,
                ap.Phone      AS Phone,
                ap.Note       AS Note,
                u.Status      AS Status
            FROM Users u
            LEFT JOIN AdminProfiles ap ON ap.UserId = u.Id
            WHERE u.Id = @UserId
              AND u.AccountType = @AccountType
            """;

        var dto = await session.Connection.QueryFirstOrDefaultAsync<AdminAccountEditDto>(
            sql, new { UserId = userId, AccountType = (short)AccountType.Admin }, session.Transaction);

        if (dto is null) return null;

        const string roleSql = """
            SELECT ur.RoleId
            FROM UserRoles ur
            INNER JOIN Roles r ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId
              AND r.Scope = @Scope
            """;

        var roleIds = await session.Connection.QueryAsync<long>(roleSql, new
        {
            UserId = userId,
            Scope = (short)RoleScope.System
        }, session.Transaction);

        return dto with { RoleIds = roleIds.AsList() };
    }

    public async Task<IReadOnlyList<AdminRoleOptionDto>> GetActiveSystemRolesAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Description
            FROM Roles
            WHERE Scope = @Scope
              AND IsActive = 1
            ORDER BY Name
            """;

        var result = await session.Connection.QueryAsync<AdminRoleOptionDto>(sql, new
        {
            Scope = (short)RoleScope.System
        }, session.Transaction);

        return result.AsList();
    }

    public async Task<User?> GetUserByEmailAsync(
        string email, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, AccountType, Name, Email, PasswordHash,
                   Status, LastLoginAt, CreatedAt, UpdatedAt
            FROM Users
            WHERE Email = @Email
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<User>(
            sql, new { Email = email }, session.Transaction);
    }

    public async Task<long> InsertUserAsync(
        User user, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Users (AccountType, Name, Email, PasswordHash, Status, CreatedAt, UpdatedAt)
            VALUES (@AccountType, @Name, @Email, @PasswordHash, @Status, GETUTCDATE(), GETUTCDATE());
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, new
        {
            AccountType = (short)user.AccountType,
            user.Name,
            user.Email,
            user.PasswordHash,
            Status = (short)user.Status
        }, session.Transaction);
    }

    public async Task UpdateUserAsync(
        User user, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Users
            SET Name      = @Name,
                Email     = @Email,
                Status    = @Status,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            user.Name,
            user.Email,
            Status = (short)user.Status,
            user.Id
        }, session.Transaction);
    }

    public async Task UpsertProfileAsync(
        long userId, string? department, string? jobTitle, string? phone, string? note,
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            IF EXISTS (SELECT 1 FROM AdminProfiles WHERE UserId = @UserId)
            BEGIN
                UPDATE AdminProfiles
                SET Department = @Department,
                    JobTitle   = @JobTitle,
                    Phone      = @Phone,
                    Note       = @Note,
                    UpdatedAt  = GETUTCDATE()
                WHERE UserId = @UserId
            END
            ELSE
            BEGIN
                INSERT INTO AdminProfiles (UserId, Department, JobTitle, Phone, Note, CreatedAt, UpdatedAt)
                VALUES (@UserId, @Department, @JobTitle, @Phone, @Note, GETUTCDATE(), GETUTCDATE())
            END
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Department = department,
            JobTitle = jobTitle,
            Phone = phone,
            Note = note
        }, session.Transaction);
    }

    public async Task ReplaceRolesAsync(
        long userId, IEnumerable<long> roleIds, IDbSession session, CancellationToken ct = default)
    {
        const string deleteSql = """
            DELETE FROM UserRoles
            WHERE UserId = @UserId
              AND RoleId IN (
                  SELECT Id FROM Roles WHERE Scope = @Scope
              )
            """;

        await session.Connection.ExecuteAsync(deleteSql, new
        {
            UserId = userId,
            Scope = (short)RoleScope.System
        }, session.Transaction);

        const string insertSql = """
            INSERT INTO UserRoles (UserId, RoleId)
            VALUES (@UserId, @RoleId)
            """;

        var distinctRoleIds = roleIds.Distinct().ToList();
        foreach (var roleId in distinctRoleIds)
        {
            await session.Connection.ExecuteAsync(insertSql, new
            {
                UserId = userId,
                RoleId = roleId
            }, session.Transaction);
        }
    }

    public async Task<long> InsertInvitationAsync(
        UserInvitation invitation, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO UserInvitations
                (UserId, Email, InvitedByUserId, RoleId, TokenHash, AccountType, Status, ExpiresAt, AcceptedAt, CreatedAt, UpdatedAt)
            VALUES
                (@UserId, @Email, @InvitedByUserId, @RoleId, @TokenHash, @AccountType, @Status, @ExpiresAt, @AcceptedAt, GETUTCDATE(), GETUTCDATE());
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, new
        {
            invitation.UserId,
            invitation.Email,
            invitation.InvitedByUserId,
            invitation.RoleId,
            invitation.TokenHash,
            AccountType = (short)invitation.AccountType,
            Status = (short)invitation.Status,
            invitation.ExpiresAt,
            invitation.AcceptedAt
        }, session.Transaction);
    }

    public async Task CancelInvitationsByUserAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE UserInvitations
            SET Status = @Cancelled,
                UpdatedAt = GETUTCDATE()
            WHERE UserId = @UserId
              AND AccountType = @AccountType
              AND Status = @Pending
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            AccountType = (short)AccountType.Admin,
            Pending = (short)InvitationStatus.Pending,
            Cancelled = (short)InvitationStatus.Cancelled
        }, session.Transaction);
    }

    public async Task<IReadOnlyList<AdminAccountLogItemDto>> GetRecentLogsAsync(
        int count, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Count)
                al.CreatedAt AS CreatedAt,
                ISNULL(actor.Name, '系統') AS ActorName,
                al.Action    AS Action,
                ISNULL(target.Name, '')    AS TargetName,
                al.Note      AS Note
            FROM ActivityLogs al
            LEFT JOIN Users actor  ON actor.Id = al.ActorUserId
            LEFT JOIN Users target ON target.Id = al.TargetId
            WHERE al.TargetType = @TargetType
            ORDER BY al.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<AdminAccountLogItemDto>(sql, new
        {
            Count = count,
            TargetType = "AdminAccounts"
        }, session.Transaction);

        return result.AsList();
    }

    public async Task<long?> GetLastActiveSystemAdminUserIdAsync(
        IDbSession session, CancellationToken ct = default)
    {
        // 簡化：只要還有任一 Active Admin 帳號即視為存在最後管理者；實際 Super Admin 角色需依 Roles 調整。
        const string sql = """
            SELECT TOP 1 u.Id
            FROM Users u
            WHERE u.AccountType = @AccountType
              AND u.Status = @Active
            ORDER BY u.Id
            """;

        return await session.Connection.ExecuteScalarAsync<long?>(sql, new
        {
            AccountType = (short)AccountType.Admin,
            Active = (short)UserStatus.Active
        }, session.Transaction);
    }

    public async Task<bool> HasSystemAdminRoleAsync(
        long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM UserRoles ur
            INNER JOIN Roles r ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId
              AND r.Scope = @Scope
            """;

        var count = await session.Connection.ExecuteScalarAsync<int>(sql, new
        {
            UserId = userId,
            Scope = (short)RoleScope.System
        }, session.Transaction);

        return count > 0;
    }
}
