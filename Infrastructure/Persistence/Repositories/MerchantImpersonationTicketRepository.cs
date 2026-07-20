using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantImpersonationTicketRepository : IMerchantImpersonationTicketRepository
{
    public async Task<long> InsertAsync(
        MerchantImpersonationTicket ticket,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO MerchantImpersonationTickets
                (TokenHash, MerchantId, AdminUserId, CreatedAtUtc, ExpiresAtUtc, UsedAtUtc, CreatedIp, UserAgent)
            VALUES
                (@TokenHash, @MerchantId, @AdminUserId, @CreatedAtUtc, @ExpiresAtUtc, NULL, @CreatedIp, @UserAgent);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                ticket.TokenHash,
                ticket.MerchantId,
                ticket.AdminUserId,
                ticket.CreatedAtUtc,
                ticket.ExpiresAtUtc,
                ticket.CreatedIp,
                ticket.UserAgent,
            },
            session.Transaction);
    }

    public async Task<MerchantImpersonationTicket?> GetByTokenHashAsync(
        string tokenHash,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, TokenHash, MerchantId, AdminUserId, CreatedAtUtc, ExpiresAtUtc, UsedAtUtc, CreatedIp, UserAgent
            FROM MerchantImpersonationTickets
            WHERE TokenHash = @TokenHash
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantImpersonationTicket>(
            sql,
            new { TokenHash = tokenHash },
            session.Transaction);
    }

    public async Task<bool> TryRedeemAsync(
        long ticketId,
        DateTime usedAtUtc,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            UPDATE MerchantImpersonationTickets
            SET UsedAtUtc = @UsedAtUtc
            WHERE Id = @TicketId
              AND UsedAtUtc IS NULL
              AND ExpiresAtUtc > SYSUTCDATETIME();
            """;

        var affected = await session.Connection.ExecuteAsync(
            sql,
            new { TicketId = ticketId, UsedAtUtc = usedAtUtc },
            session.Transaction);

        return affected == 1;
    }
}
