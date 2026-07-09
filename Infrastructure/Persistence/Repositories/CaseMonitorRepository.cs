using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Pagination;
using Dapper;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class CaseMonitorRepository : ICaseMonitorRepository
{
    // ── GetListAsync ──────────────────────────────────────────────
    public async Task<(IReadOnlyList<CaseListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        CaseStatus? status,
        bool? hasPendingReview,
        bool? hasCommission,
        DateTime? dateFrom,
        DateTime? dateTo,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default)
    {
        var where = BuildListWhere(keyword, status, hasPendingReview, hasCommission, dateFrom, dateTo);

        var countSql = $"""
            SELECT COUNT(DISTINCT c.Id)
            FROM Cases c
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            LEFT JOIN Tasks t ON t.CaseId = c.Id
            LEFT JOIN Disputes disp ON disp.CaseId = c.Id AND disp.Status IN (1, 2)
            {where}
            """;

        var listSql = $"""
            SELECT
                c.Id                                                                        AS CaseId,
                c.Title,
                m.Id                                                                        AS MerchantId,
                m.CompanyName                                                               AS MerchantName,
                c.Status,
                CASE WHEN c.CashRewardAmount > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END  AS HasCash,
                c.IsCommissionEnabled                                                       AS HasCommission,
                CASE WHEN EXISTS(
                    SELECT 1 FROM CaseBarterItems bi WHERE bi.CaseId = c.Id
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END                             AS HasBarter,
                c.WantedKolCount,
                c.ApplicationCount,
                c.ApprovedAssignmentCount,
                COUNT(CASE WHEN t.Status = 3 THEN 1 END)                                  AS TaskInProgressCount,
                COUNT(CASE WHEN t.Status = 4 THEN 1 END)                                  AS TaskUnderReviewCount,
                COUNT(CASE WHEN t.Status = 6 THEN 1 END)                                  AS TaskCompletedCount,
                COUNT(CASE WHEN t.Status = 7 THEN 1 END)                                  AS TaskIncompleteCount,
                COUNT(CASE WHEN t.Status = 8 THEN 1 END)                                  AS TaskCancelledCount,
                COUNT(DISTINCT disp.Id)                                                    AS TaskDisputeCount,
                c.CreatedAt
            FROM Cases c
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            LEFT JOIN Tasks t ON t.CaseId = c.Id
            LEFT JOIN Disputes disp ON disp.CaseId = c.Id AND disp.Status IN (1, 2)
            {where}
            GROUP BY
                c.Id, c.Title, m.Id, m.CompanyName, c.Status,
                c.CashRewardAmount, c.IsCommissionEnabled,
                c.WantedKolCount, c.ApplicationCount, c.ApprovedAssignmentCount,
                c.CreatedAt
            ORDER BY c.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = (short?)status,
            HasCommission = hasCommission,
            DateFrom = dateFrom,
            DateTo = dateTo,
            pageQuery.Offset,
            pageQuery.PageSize
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        var items = await session.Connection.QueryAsync<CaseListItemDto>(
            listSql, param, session.Transaction);

        return (items.AsList(), totalCount);
    }

    // ── GetSummaryAsync ───────────────────────────────────────────
    public async Task<CaseSummaryDto> GetSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*)                                               AS TotalCount,
                COUNT(CASE WHEN Status = 1 THEN 1 END)                AS DraftCount,
                COUNT(CASE WHEN Status = 2 THEN 1 END)                AS RecruitingCount,
                COUNT(CASE WHEN Status = 3 THEN 1 END)                AS RecruitmentClosedCount,
                COUNT(CASE WHEN Status = 4 THEN 1 END)                AS InProgressCount,
                COUNT(CASE WHEN Status IN (5, 6) THEN 1 END)          AS CompletedCount,
                COUNT(CASE WHEN Status = 7 THEN 1 END)                AS CancelledCount
            FROM Cases
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseSummaryDto>(
            sql, transaction: session.Transaction) ?? new CaseSummaryDto();
    }

    // ── GetAlertAsync ─────────────────────────────────────────────
    public async Task<CaseAlertDto> GetAlertAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM Tasks WHERE Status = 4)                              AS PendingReviewTaskCount,
                (SELECT COUNT(t.Id)
                 FROM Tasks t
                 INNER JOIN Cases c ON c.Id = t.CaseId
                 WHERE t.Status = 3 AND c.SubmissionDeadline < GETUTCDATE())               AS OverdueTaskCount,
                (SELECT COUNT(*) FROM Disputes WHERE Status IN (1, 2))                     AS DisputeTaskCount,
                0                                                                          AS CommissionSyncErrorCount
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseAlertDto>(
            sql, transaction: session.Transaction) ?? new CaseAlertDto();
    }

    // ── private ───────────────────────────────────────────────────
    private static string BuildListWhere(
        string? keyword,
        CaseStatus? status,
        bool? hasPendingReview,
        bool? hasCommission,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        var clauses = new List<string>();

        if (!string.IsNullOrWhiteSpace(keyword))
            clauses.Add("(c.Title LIKE @Keyword OR m.CompanyName LIKE @Keyword)");

        if (status.HasValue)
            clauses.Add("c.Status = @Status");

        if (hasPendingReview == true)
            clauses.Add("EXISTS(SELECT 1 FROM Tasks pt WHERE pt.CaseId = c.Id AND pt.Status = 4)");
        else if (hasPendingReview == false)
            clauses.Add("NOT EXISTS(SELECT 1 FROM Tasks pt WHERE pt.CaseId = c.Id AND pt.Status = 4)");

        if (hasCommission.HasValue)
            clauses.Add("c.IsCommissionEnabled = @HasCommission");

        if (dateFrom.HasValue)
            clauses.Add("c.CreatedAt >= @DateFrom");

        if (dateTo.HasValue)
            clauses.Add("c.CreatedAt < DATEADD(DAY, 1, @DateTo)");

        return clauses.Count == 0 ? "" : "WHERE " + string.Join(" AND ", clauses);
    }
}
