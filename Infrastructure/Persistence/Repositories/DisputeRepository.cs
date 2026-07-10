using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Disputes.DTOs;
using Common.Pagination;
using Dapper;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class DisputeRepository : IDisputeRepository
{
    public async Task<(IReadOnlyList<DisputeListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        DisputeStatus? status,
        string? disputeType,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default)
    {
        var where = BuildListWhere(keyword, status, disputeType);

        var countSql = $"""
            SELECT COUNT(*)
            FROM Disputes d
            INNER JOIN Cases c ON c.Id = d.CaseId
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            INNER JOIN Tasks t ON t.Id = d.TaskId
            LEFT JOIN KolProfiles kp ON kp.Id = t.KolId
            {where}
            """;

        var listSql = $"""
            SELECT
                d.Id                    AS DisputeId,
                'DSP-' + FORMAT(d.OpenedAt, 'yyyyMM') + '-' + CAST(d.Id AS VARCHAR(20)) AS DisputeNo,
                c.Id                    AS CaseId,
                'CASE-' + FORMAT(c.CreatedAt, 'yyyyMM') + '-' + CAST(c.Id AS VARCHAR(20)) AS CaseNo,
                c.Title                 AS CaseTitle,
                m.Id                    AS MerchantId,
                m.CompanyName           AS MerchantName,
                kp.Id                   AS KolId,
                kp.DisplayName          AS KolName,
                d.Status,
                d.Reason                AS DisputeType,
                t.Status                AS TaskStatus,
                d.OpenedAt
            FROM Disputes d
            INNER JOIN Cases c ON c.Id = d.CaseId
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            INNER JOIN Tasks t ON t.Id = d.TaskId
            LEFT JOIN KolProfiles kp ON kp.Id = t.KolId
            {where}
            ORDER BY d.OpenedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = (short?)status,
            DisputeType = string.IsNullOrWhiteSpace(disputeType) ? null : disputeType.Trim(),
            pageQuery.Offset,
            pageQuery.PageSize
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(countSql, param, session.Transaction);
        var items = await session.Connection.QueryAsync<DisputeListItemDto>(listSql, param, session.Transaction);

        return (items.AsList(), totalCount);
    }

    public async Task<DisputeSummaryDto> GetSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*)                                                   AS TotalCount,
                COUNT(CASE WHEN Status = 1 THEN 1 END)                     AS OpenCount,
                COUNT(CASE WHEN Status = 2 THEN 1 END)                     AS UnderReviewCount,
                COUNT(CASE WHEN Status IN (3, 4, 5) THEN 1 END)            AS ResolvedCount,
                COUNT(CASE WHEN Status = 6 THEN 1 END)                     AS CancelledCount,
                COUNT(CASE WHEN CAST(OpenedAt AS DATE) = CAST(GETUTCDATE() AS DATE) THEN 1 END) AS TodayNewCount
            FROM Disputes
            """;

        var raw = await session.Connection.QueryFirstOrDefaultAsync<DisputeSummaryDto>(
            sql, transaction: session.Transaction) ?? new DisputeSummaryDto();

        // 待補件 = 狀態未結案且任務狀態為 RevisionRequested
        const string revisionSql = """
            SELECT COUNT(*)
            FROM Disputes d
            INNER JOIN Tasks t ON t.Id = d.TaskId
            WHERE d.Status IN (1, 2)
              AND t.Status = 5
            """;
        var revisionCount = await session.Connection.ExecuteScalarAsync<int>(
            revisionSql, transaction: session.Transaction);

        return new DisputeSummaryDto
        {
            TotalCount = raw.TotalCount,
            OpenCount = raw.OpenCount,
            UnderReviewCount = raw.UnderReviewCount,
            RevisionCount = revisionCount,
            ResolvedCount = raw.ResolvedCount,
            TodayNewCount = raw.TodayNewCount
        };
    }

    private static string BuildListWhere(string? keyword, DisputeStatus? status, string? disputeType)
    {
        var clauses = new List<string>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            clauses.Add("""
                (
                    d.Reason LIKE @Keyword
                    OR c.Title LIKE @Keyword
                    OR m.CompanyName LIKE @Keyword
                    OR kp.DisplayName LIKE @Keyword
                    OR CAST(d.Id AS VARCHAR(20)) LIKE @Keyword
                )
                """);
        }

        if (status.HasValue)
        {
            clauses.Add("d.Status = @Status");
        }

        if (!string.IsNullOrWhiteSpace(disputeType))
        {
            clauses.Add("d.Reason = @DisputeType");
        }

        return clauses.Count == 0
            ? string.Empty
            : "WHERE " + string.Join(" AND ", clauses);
    }
}
