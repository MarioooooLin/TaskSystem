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

    public async Task<DisputeDetailDto?> GetDetailAsync(
        long disputeId,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                d.Id                    AS DisputeId,
                'DSP-' + FORMAT(d.OpenedAt, 'yyyyMM') + '-' + CAST(d.Id AS VARCHAR(20)) AS DisputeNo,
                d.Reason                AS DisputeType,
                d.Status,
                c.Id                    AS CaseId,
                'CASE-' + FORMAT(c.CreatedAt, 'yyyyMM') + '-' + CAST(c.Id AS VARCHAR(20)) AS CaseNo,
                c.Title                 AS CaseTitle,
                c.DeliverableDescription AS MerchantRequirement,
                c.Description           AS CaseSummary,
                m.Id                    AS MerchantId,
                m.CompanyName           AS MerchantName,
                kp.Id                   AS KolId,
                kp.DisplayName          AS KolName,
                d.Description           AS KolDisputeReason,
                s.Note                  AS KolSubmission,
                s.RejectReason          AS MerchantRejectionReason,
                d.OpenedAt
            FROM Disputes d
            INNER JOIN Cases c ON c.Id = d.CaseId
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            INNER JOIN Tasks t ON t.Id = d.TaskId
            LEFT JOIN KolProfiles kp ON kp.Id = t.KolId
            LEFT JOIN Submissions s ON s.TaskId = t.Id
            WHERE d.Id = @DisputeId
            ORDER BY s.SubmittedAt DESC
            """;

        var detail = await session.Connection.QueryFirstOrDefaultAsync<DisputeDetailDto>(
            sql, new { DisputeId = disputeId }, session.Transaction);

        if (detail is null) return null;

        detail.MerchantContact = await GetMerchantContactAsync(detail.MerchantId, session);
        detail.KolContact = await GetKolContactAsync(detail.KolId, session);
        detail.KolSubmissionItems = await GetSubmissionItemsAsync(detail.CaseId, detail.KolId, session);
        detail.Timeline = await GetTimelineAsync(disputeId, session);

        return detail;
    }

    private static async Task<DisputeContactDto> GetMerchantContactAsync(
        long merchantId,
        IDbSession session)
    {
        const string primarySql = """
            SELECT TOP 1
                mc.Name     AS Name,
                mc.Phone    AS Mobile,
                mc.Phone    AS Phone,
                mc.Email    AS Email
            FROM MerchantContacts mc
            WHERE mc.MerchantId = @MerchantId
            ORDER BY mc.CreatedAt ASC
            """;

        var contact = await session.Connection.QueryFirstOrDefaultAsync<DisputeContactDto>(
            primarySql, new { MerchantId = merchantId }, session.Transaction);

        if (contact is not null) return contact;

        const string fallbackSql = """
            SELECT TOP 1
                m.ContactName AS Name,
                m.Phone       AS Mobile,
                m.Phone       AS Phone,
                m.CompanyEmail AS Email
            FROM Merchants m
            WHERE m.Id = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<DisputeContactDto>(
            fallbackSql, new { MerchantId = merchantId }, session.Transaction)
            ?? new DisputeContactDto();
    }

    private static async Task<DisputeContactDto> GetKolContactAsync(
        long? kolId,
        IDbSession session)
    {
        if (!kolId.HasValue) return new DisputeContactDto();

        const string sql = """
            SELECT
                kp.RealName  AS Name,
                kp.Phone     AS Mobile,
                kp.Phone     AS Phone,
                u.Email      AS Email,
                kp.LineContactId AS LineStatus
            FROM KolProfiles kp
            INNER JOIN Users u ON u.Id = kp.UserId
            WHERE kp.Id = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<DisputeContactDto>(
            sql, new { KolId = kolId.Value }, session.Transaction)
            ?? new DisputeContactDto();
    }

    private static async Task<IReadOnlyList<DisputeSubmissionItemDto>> GetSubmissionItemsAsync(
        long caseId,
        long? kolId,
        IDbSession session)
    {
        if (!kolId.HasValue) return Array.Empty<DisputeSubmissionItemDto>();

        const string sql = """
            SELECT
                si.Platform,
                si.Url,
                si.Note
            FROM SubmissionItems si
            INNER JOIN Submissions s ON s.Id = si.SubmissionId
            INNER JOIN Tasks t ON t.Id = s.TaskId
            WHERE t.CaseId = @CaseId
              AND t.KolId = @KolId
            ORDER BY si.Id
            """;

        var items = await session.Connection.QueryAsync<DisputeSubmissionItemDto>(
            sql, new { CaseId = caseId, KolId = kolId.Value }, session.Transaction);

        return items.AsList();
    }

    private static async Task<IReadOnlyList<DisputeTimelineDto>> GetTimelineAsync(
        long disputeId,
        IDbSession session)
    {
        const string sql = """
            SELECT CreatedAt, Message AS Text, 0 AS IsCurrent
            FROM DisputeMessages
            WHERE DisputeId = @DisputeId
            UNION ALL
            SELECT CreatedAt, Action + ISNULL(': ' + Note, '') AS Text, 0 AS IsCurrent
            FROM ActivityLogs
            WHERE TargetType = 'Disputes' AND TargetId = @DisputeId
            ORDER BY CreatedAt ASC
            """;

        var items = await session.Connection.QueryAsync<DisputeTimelineDto>(
            sql, new { DisputeId = disputeId }, session.Transaction);

        var list = items.AsList();
        if (list.Count > 0)
        {
            list[^1].IsCurrent = true;
        }

        return list;
    }

    public async Task<bool> ResolveAsync(
        long disputeId,
        DisputeStatus status,
        long resolvedByAdminId,
        string resolutionNote,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Disputes
            SET Status = @Status,
                ResolvedByAdminId = @ResolvedByAdminId,
                ResolutionNote = @ResolutionNote,
                ResolvedAt = GETUTCDATE()
            WHERE Id = @DisputeId
            """;

        var rows = await session.Connection.ExecuteAsync(sql, new
        {
            DisputeId = disputeId,
            Status = (short)status,
            ResolvedByAdminId = resolvedByAdminId,
            ResolutionNote = resolutionNote
        }, session.Transaction);

        return rows > 0;
    }
}
