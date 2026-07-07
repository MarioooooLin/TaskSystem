using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantMemberRepository : IMerchantMemberRepository
{
    public async Task<MerchantMember?> GetByIdAsync(
        long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, MerchantId, UserId, RoleId, Status, JoinedAt, UpdatedAt
            FROM MerchantMembers
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantMember>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<MerchantMember?> GetByMerchantAndUserAsync(
        long merchantId, long userId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, MerchantId, UserId, RoleId, Status, JoinedAt, UpdatedAt
            FROM MerchantMembers
            WHERE MerchantId = @MerchantId AND UserId = @UserId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantMember>(
            sql, new { MerchantId = merchantId, UserId = userId }, session.Transaction);
    }

    public async Task<IReadOnlyList<MerchantMemberItemDto>> GetMemberListAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                mm.Id           AS MemberId,
                mm.UserId,
                u.DisplayName   AS UserName,
                u.Email         AS UserEmail,
                r.Name          AS RoleName,
                mm.Status,
                mm.JoinedAt
            FROM MerchantMembers mm
            JOIN Users u ON u.Id = mm.UserId
            JOIN Roles r ON r.Id = mm.RoleId
            WHERE mm.MerchantId = @MerchantId
            ORDER BY mm.JoinedAt ASC
            """;

        var result = await session.Connection.QueryAsync<MerchantMemberItemDto>(
            sql, new { MerchantId = merchantId }, session.Transaction);

        return result.AsList();
    }

    public async Task<long> InsertAsync(
        MerchantMember member, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO MerchantMembers (MerchantId, UserId, RoleId, Status)
            VALUES (@MerchantId, @UserId, @RoleId, @Status);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, member, session.Transaction);
    }

    public async Task UpdateAsync(
        MerchantMember member, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE MerchantMembers SET
                RoleId      = @RoleId,
                Status      = @Status,
                UpdatedAt   = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, member, session.Transaction);
    }
}
