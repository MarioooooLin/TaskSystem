using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class KolStatsRepository : IKolStatsRepository
{
    // ── GetStatsByKolIdAsync ──────────────────────────────────────
    public async Task<KolStatsDto> GetStatsByKolIdAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*)                                                    AS TaskCount,
                SUM(CASE WHEN t.Status = 6 THEN 1 ELSE 0 END)             AS CompletedTaskCount,
                SUM(CASE WHEN t.Status = 4 THEN 1 ELSE 0 END)             AS PendingReviewCount,
                (SELECT COUNT(*) FROM Disputes d
                 JOIN Tasks dt ON dt.Id = d.TaskId
                 WHERE dt.KolId = @KolId
                   AND d.Status IN (1, 2))                                 AS DisputeCount,
                SUM(CASE WHEN t.Status = 8 AND t.CancellationSource = 1 THEN 1 ELSE 0 END) AS AbandonedTaskCount
            FROM Tasks t
            WHERE t.KolId = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<KolStatsDto>(
            sql, new { KolId = kolId }, session.Transaction) ?? new KolStatsDto();
    }

    // ── GetEarningsSummaryAsync ───────────────────────────────────
    public async Task<KolEarningsSummaryDto> GetEarningsSummaryAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ISNULL(PendingAmount, 0)    AS PendingAmount,
                ISNULL(AvailableAmount, 0)  AS AvailableAmount,
                ISNULL(PaidAmount, 0)       AS PaidAmount,
                (SELECT ISNULL(SUM(ke.NetAmount), 0)
                 FROM KolEarnings ke
                 WHERE ke.KolId = @KolId AND ke.Status = 4) AS RequestedAmount
            FROM KolWallets
            WHERE KolId = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<KolEarningsSummaryDto>(
            sql, new { KolId = kolId }, session.Transaction) ?? new KolEarningsSummaryDto();
    }

    // ── GetRecentTasksAsync ───────────────────────────────────────
    public async Task<IReadOnlyList<KolTaskSummaryDto>> GetRecentTasksAsync(
        long kolId, int take, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                t.Id            AS TaskId,
                t.CaseId,
                c.Title         AS CaseTitle,
                t.Status        AS TaskStatus,
                m.CompanyName   AS MerchantName,
                c.CashRewardAmount,
                t.CompletedAt,
                c.CreatedAt
            FROM Tasks t
            JOIN Cases c     ON c.Id = t.CaseId
            JOIN Merchants m ON m.Id = c.MerchantId
            WHERE t.KolId = @KolId
            ORDER BY c.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<KolTaskSummaryDto>(
            sql, new { KolId = kolId, Take = take }, session.Transaction);

        return result.AsList();
    }

    // ── GetRecentActivityLogsAsync ────────────────────────────────
    public async Task<IReadOnlyList<KolActivityLogDto>> GetRecentActivityLogsAsync(
        long kolId, int take, IDbSession session, CancellationToken ct = default)
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
            WHERE al.TargetType = 'KolProfiles' AND al.TargetId = @KolId
            ORDER BY al.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<KolActivityLogDto>(
            sql, new { KolId = kolId, Take = take }, session.Transaction);

        return result.AsList();
    }

    // ── GetCategoriesAsync ────────────────────────────────────────
    public async Task<IReadOnlyList<short>> GetCategoriesAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Category FROM KolCategories
            WHERE KolId = @KolId
            ORDER BY Category ASC
            """;

        var result = await session.Connection.QueryAsync<short>(
            sql, new { KolId = kolId }, session.Transaction);

        return result.AsList();
    }
}
