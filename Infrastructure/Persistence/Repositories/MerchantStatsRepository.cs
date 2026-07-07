using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantStatsRepository : IMerchantStatsRepository
{
    // ── GetStatsByMerchantIdAsync ─────────────────────────────────
    public async Task<MerchantStatsDto> GetStatsByMerchantIdAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(DISTINCT c.Id)                                                AS CaseCount,
                COUNT(t.Id)                                                         AS TaskCount,
                COUNT(DISTINCT d.Id)                                                AS DisputeCount,
                CASE
                    WHEN COUNT(t.Id) = 0 THEN 0
                    ELSE CAST(SUM(CASE WHEN t.Status = 5 THEN 1 ELSE 0 END) AS DECIMAL(5,2))
                         / COUNT(t.Id) * 100
                END                                                                  AS CompletionRate
            FROM Cases c
            LEFT JOIN Tasks t      ON t.CaseId = c.Id
            LEFT JOIN Disputes d   ON d.CaseId = c.Id
            WHERE c.MerchantId = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantStatsDto>(
            sql, new { MerchantId = merchantId }, session.Transaction) ?? new MerchantStatsDto();
    }

    // ── GetRecentCasesAsync ───────────────────────────────────────
    public async Task<IReadOnlyList<MerchantCaseSummaryDto>> GetRecentCasesAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                c.Id                        AS CaseId,
                c.Title,
                c.Status,
                c.CashRewardAmount,
                c.WantedKolCount,
                c.ApprovedAssignmentCount,
                c.CreatedAt
            FROM Cases c
            WHERE c.MerchantId = @MerchantId
            ORDER BY c.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<MerchantCaseSummaryDto>(
            sql, new { MerchantId = merchantId, Take = take }, session.Transaction);

        return result.AsList();
    }

    // ── GetRecentActivityLogsAsync ────────────────────────────────
    public async Task<IReadOnlyList<MerchantActivityLogDto>> GetRecentActivityLogsAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                al.Id,
                al.Action,
                u.Name          AS ActorName,
                al.CaseId       AS RelatedCaseId,
                al.Note,
                al.CreatedAt
            FROM ActivityLogs al
            LEFT JOIN Users u ON u.Id = al.ActorUserId
            WHERE al.TargetType = 'Merchants' AND al.TargetId = @MerchantId
            ORDER BY al.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<MerchantActivityLogDto>(
            sql, new { MerchantId = merchantId, Take = take }, session.Transaction);

        return result.AsList();
    }
}
