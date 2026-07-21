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

    // ── GetDetailAsync ────────────────────────────────────────────
    public async Task<CaseDetailDto?> GetDetailAsync(
        long caseId, IDbSession session, CancellationToken ct = default)
    {
        var conn = session.Connection;
        var tx = session.Transaction;

        // 1. 主案件資料
        const string caseSql = """
            SELECT
                c.Id                    AS CaseId,
                c.Title,
                c.Status,
                m.Id                    AS MerchantId,
                m.CompanyName           AS MerchantName,
                CASE WHEN c.CashRewardAmount > 0
                     THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END           AS HasCash,
                CASE WHEN EXISTS(SELECT 1 FROM CaseBarterItems WHERE CaseId = c.Id)
                     THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END           AS HasBarter,
                c.IsCommissionEnabled                                       AS HasCommission,
                c.CashRewardAmount,
                c.CommissionRate,
                c.CookieDays,
                c.ApplicationDeadline,
                c.SubmissionDeadline,
                c.StartedAt,
                c.CompletedAt,
                c.WantedKolCount,
                c.ApplicationCount,
                c.ApprovedAssignmentCount,
                u.Name                  AS CreatedByName,
                c.CreatedAt
            FROM Cases c
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            INNER JOIN Users u ON u.Id = c.CreatedByUserId
            WHERE c.Id = @CaseId
            """;

        var raw = await conn.QueryFirstOrDefaultAsync<CaseRawRow>(caseSql, new { CaseId = caseId }, tx);
        if (raw is null) return null;

        // 2. 任務統計
        const string statsSql = """
            SELECT
                COUNT(CASE WHEN t.Status = 3 THEN 1 END)   AS TaskInProgressCount,
                COUNT(CASE WHEN t.Status = 4 THEN 1 END)   AS TaskUnderReviewCount,
                COUNT(CASE WHEN t.Status = 5 THEN 1 END)   AS TaskRevisionCount,
                COUNT(CASE WHEN t.Status = 6 THEN 1 END)   AS TaskCompletedCount,
                COUNT(CASE WHEN t.Status = 7 THEN 1 END)   AS TaskIncompleteCount,
                COUNT(CASE WHEN t.Status = 8 THEN 1 END)   AS TaskCancelledCount,
                COUNT(DISTINCT d.Id)                        AS TaskDisputeCount
            FROM Tasks t
            LEFT JOIN Disputes d ON d.TaskId = t.Id AND d.Status IN (1, 2)
            WHERE t.CaseId = @CaseId
            """;
        var stats = await conn.QueryFirstAsync<TaskStatsRow>(statsSql, new { CaseId = caseId }, tx);

        // 3. 未入選人數（Rejected / Cancelled / Invalid）
        var rejectedCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CaseApplications WHERE CaseId = @CaseId AND Status IN (4, 5, 6)",
            new { CaseId = caseId }, tx);

        // 4. 曝光平台
        var platforms = (await conn.QueryAsync<short>(
            "SELECT Platform FROM CasePlatforms WHERE CaseId = @CaseId ORDER BY Id",
            new { CaseId = caseId }, tx)).AsList();

        // 5. 體驗項目
        var barterItems = (await conn.QueryAsync<string>(
            "SELECT Name FROM CaseBarterItems WHERE CaseId = @CaseId ORDER BY Id",
            new { CaseId = caseId }, tx)).AsList();

        // 6. 任務清單（含 KOL、最新 Submission、Dispute）
        const string taskSql = """
            ;WITH LatestSub AS (
                SELECT s.TaskId, s.Id AS SubmissionId, s.Status,
                       ROW_NUMBER() OVER (PARTITION BY s.TaskId ORDER BY s.SubmittedAt DESC) AS rn
                FROM Submissions s
                INNER JOIN Tasks t2 ON t2.Id = s.TaskId
                WHERE t2.CaseId = @CaseId
            )
            SELECT
                t.Id                    AS TaskId,
                t.Status                AS TaskStatus,
                kp.Id                   AS KolId,
                kp.DisplayName          AS KolName,
                (SELECT TOP 1 Platform
                 FROM KolSocialAccounts
                 WHERE KolId = kp.Id ORDER BY Id)           AS KolMainPlatform,
                (SELECT TOP 1 Category
                 FROM KolCategories
                 WHERE KolId = kp.Id ORDER BY Id)           AS KolFirstCategory,
                ls.Status               AS SubmissionStatus,
                (SELECT TOP 1 Url
                 FROM SubmissionItems
                 WHERE SubmissionId = ls.SubmissionId
                   AND Url IS NOT NULL)                     AS SubmissionUrl,
                CASE WHEN d.Id IS NOT NULL
                     THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasDispute,
                d.Status                AS DisputeStatus,
                ISNULL(t.CompletedAt, ISNULL(t.SubmittedAt, t.StartedAt)) AS UpdatedAt
            FROM Tasks t
            LEFT JOIN KolProfiles kp ON kp.Id = t.KolId
            LEFT JOIN LatestSub ls ON ls.TaskId = t.Id AND ls.rn = 1
            LEFT JOIN Disputes d ON d.TaskId = t.Id AND d.Status IN (1, 2)
            WHERE t.CaseId = @CaseId
            ORDER BY t.Id
            """;
        var tasks = (await conn.QueryAsync<CaseTaskListItemDto>(taskSql, new { CaseId = caseId }, tx)).AsList();

        // 7. 附件
        const string filesSql = """
            SELECT
                f.Id        AS FileId,
                f.FileName,
                f.FileSize,
                f.MimeType,
                f.CreatedAt AS UploadedAt,
                ca.Type     AS AttachmentType
            FROM CaseAttachments ca
            INNER JOIN Files f ON f.Id = ca.FileId
            WHERE ca.CaseId = @CaseId
            ORDER BY f.CreatedAt DESC
            """;
        var attachments = (await conn.QueryAsync<CaseAttachmentDto>(filesSql, new { CaseId = caseId }, tx)).AsList();

        // 8. 操作紀錄（最新 10 筆）
        const string logSql = """
            SELECT TOP 10
                al.Id,
                al.Action,
                al.Note,
                u.Name  AS ActorName,
                al.CreatedAt
            FROM ActivityLogs al
            LEFT JOIN Users u ON u.Id = al.ActorUserId
            WHERE al.CaseId = @CaseId
            ORDER BY al.CreatedAt DESC
            """;
        var logs = (await conn.QueryAsync<CaseActivityLogDto>(logSql, new { CaseId = caseId }, tx)).AsList();

        return new CaseDetailDto
        {
            CaseId = raw.CaseId,
            Title = raw.Title,
            Status = raw.Status,
            MerchantId = raw.MerchantId,
            MerchantName = raw.MerchantName,
            HasCash = raw.HasCash,
            HasBarter = raw.HasBarter,
            HasCommission = raw.HasCommission,
            CashRewardAmount = raw.CashRewardAmount,
            BarterItems = barterItems,
            CommissionRate = raw.CommissionRate,
            CookieDays = raw.CookieDays,
            ApplicationDeadline = raw.ApplicationDeadline,
            SubmissionDeadline = raw.SubmissionDeadline,
            StartedAt = raw.StartedAt,
            CompletedAt = raw.CompletedAt,
            WantedKolCount = raw.WantedKolCount,
            ApplicationCount = raw.ApplicationCount,
            ApprovedAssignmentCount = raw.ApprovedAssignmentCount,
            RejectedApplicationCount = rejectedCount,
            TaskInProgressCount = stats.TaskInProgressCount,
            TaskUnderReviewCount = stats.TaskUnderReviewCount,
            TaskRevisionCount = stats.TaskRevisionCount,
            TaskCompletedCount = stats.TaskCompletedCount,
            TaskIncompleteCount = stats.TaskIncompleteCount,
            TaskCancelledCount = stats.TaskCancelledCount,
            TaskDisputeCount = stats.TaskDisputeCount,
            CreatedByName = raw.CreatedByName,
            Platforms = platforms,
            CreatedAt = raw.CreatedAt,
            Tasks = tasks,
            Attachments = attachments,
            ActivityLogs = logs
        };
    }

    // ── GetMerchantListAsync ──────────────────────────────────────
    public async Task<(IReadOnlyList<MerchantCaseListItemDto> Items, int TotalCount)> GetMerchantListAsync(
        long merchantId,
        string? keyword,
        CaseStatus? status,
        bool? closedOnly,
        int? rewardTypeFilter,
        SocialPlatform? platform,
        DateTime? dateFrom,
        DateTime? dateTo,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default)
    {
        var where = BuildMerchantListWhere(keyword, status, closedOnly, rewardTypeFilter, platform, dateFrom, dateTo);

        var countSql = $"""
            SELECT COUNT(*)
            FROM Cases c
            {where}
            """;

        var listSql = $"""
            SELECT
                c.Id                                                                        AS CaseId,
                c.Title,
                c.Status,
                CASE WHEN c.CashRewardAmount > 0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END  AS HasCash,
                CASE WHEN EXISTS(SELECT 1 FROM CaseBarterItems bi WHERE bi.CaseId = c.Id)
                     THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END                            AS HasBarter,
                c.IsCommissionEnabled                                                       AS HasCommission,
                c.WantedKolCount,
                c.ApplicationCount,
                c.ApprovedAssignmentCount,
                COUNT(CASE WHEN t.Status = 4 THEN 1 END)                                    AS TaskUnderReviewCount,
                COUNT(CASE WHEN t.Status = 3 THEN 1 END)                                    AS TaskInProgressCount,
                COUNT(CASE WHEN t.Status = 6 THEN 1 END)                                    AS TaskCompletedCount,
                COUNT(CASE WHEN t.Status = 7 THEN 1 END)                                    AS TaskIncompleteCount,
                COUNT(CASE WHEN t.Status = 8 THEN 1 END)                                    AS TaskCancelledCount,
                c.CashRewardAmount,
                c.ApplicationDeadline,
                c.SubmissionDeadline,
                c.PublishedAt,
                c.CreatedAt
            FROM Cases c
            LEFT JOIN Tasks t ON t.CaseId = c.Id
            {where}
            GROUP BY
                c.Id, c.Title, c.Status, c.CashRewardAmount,
                c.IsCommissionEnabled, c.WantedKolCount, c.ApplicationCount,
                c.ApprovedAssignmentCount, c.ApplicationDeadline,
                c.SubmissionDeadline, c.PublishedAt, c.CreatedAt
            ORDER BY c.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            MerchantId = merchantId,
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = (short?)status,
            ClosedOnly = closedOnly,
            RewardTypeFilter = rewardTypeFilter,
            Platform = (short?)platform,
            DateFrom = dateFrom,
            DateTo = dateTo,
            pageQuery.Offset,
            pageQuery.PageSize
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        var items = (await session.Connection.QueryAsync<MerchantCaseListItemDto>(
            listSql, param, session.Transaction)).AsList();

        if (items.Count > 0)
        {
            var caseIds = items.Select(i => i.CaseId).ToList();
            var platformRows = await session.Connection.QueryAsync<(long CaseId, short Platform)>(
                "SELECT CaseId, Platform FROM CasePlatforms WHERE CaseId IN @CaseIds ORDER BY CaseId, Platform",
                new { CaseIds = caseIds }, session.Transaction);

            var platformLookup = platformRows
                .GroupBy(r => r.CaseId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<SocialPlatform>)g.Select(r => (SocialPlatform)r.Platform).ToList());

            foreach (var item in items)
            {
                if (platformLookup.TryGetValue(item.CaseId, out var platforms))
                    item.Platforms = platforms;
            }
        }

        return (items, totalCount);
    }

    // ── GetMerchantSummaryAsync ─────────────────────────────────────
    public async Task<MerchantCaseSummaryDto> GetMerchantSummaryAsync(
        long merchantId,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*)                                      AS TotalCount,
                COUNT(CASE WHEN Status = 1 THEN 1 END)        AS DraftCount,
                COUNT(CASE WHEN Status = 2 THEN 1 END)        AS RecruitingCount,
                COUNT(CASE WHEN Status = 4 THEN 1 END)        AS InProgressCount,
                COUNT(CASE WHEN Status = 5 THEN 1 END)        AS PendingAcceptanceCount,
                COUNT(CASE WHEN Status IN (6, 7) THEN 1 END)  AS ClosedCount
            FROM Cases
            WHERE MerchantId = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantCaseSummaryDto>(
            sql, new { MerchantId = merchantId }, session.Transaction) ?? new MerchantCaseSummaryDto();
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

    private static string BuildMerchantListWhere(
        string? keyword,
        CaseStatus? status,
        bool? closedOnly,
        int? rewardTypeFilter,
        SocialPlatform? platform,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        var clauses = new List<string> { "c.MerchantId = @MerchantId" };

        if (!string.IsNullOrWhiteSpace(keyword))
            clauses.Add("c.Title LIKE @Keyword");

        if (closedOnly == true)
            clauses.Add("c.Status IN (6, 7)");
        else if (status.HasValue)
            clauses.Add("c.Status = @Status");

        switch (rewardTypeFilter)
        {
            case 1:
                clauses.Add("c.CashRewardAmount > 0 AND NOT EXISTS(SELECT 1 FROM CaseBarterItems bi WHERE bi.CaseId = c.Id)");
                break;
            case 2:
                clauses.Add("c.CashRewardAmount = 0 AND EXISTS(SELECT 1 FROM CaseBarterItems bi WHERE bi.CaseId = c.Id)");
                break;
            case 3:
                clauses.Add("c.CashRewardAmount > 0 AND EXISTS(SELECT 1 FROM CaseBarterItems bi WHERE bi.CaseId = c.Id)");
                break;
        }

        if (platform.HasValue)
            clauses.Add("EXISTS(SELECT 1 FROM CasePlatforms cp WHERE cp.CaseId = c.Id AND cp.Platform = @Platform)");

        if (dateFrom.HasValue)
            clauses.Add("c.CreatedAt >= @DateFrom");

        if (dateTo.HasValue)
            clauses.Add("c.CreatedAt < DATEADD(DAY, 1, @DateTo)");

        return "WHERE " + string.Join(" AND ", clauses);
    }

    // ── Private mapping types ─────────────────────────────────────
    private sealed record CaseRawRow(
        long CaseId, string Title, CaseStatus Status,
        long MerchantId, string MerchantName,
        bool HasCash, bool HasBarter, bool HasCommission,
        decimal CashRewardAmount, decimal? CommissionRate, int? CookieDays,
        DateTime ApplicationDeadline, DateTime SubmissionDeadline,
        DateTime? StartedAt, DateTime? CompletedAt,
        int WantedKolCount, int ApplicationCount, int ApprovedAssignmentCount,
        string? CreatedByName, DateTime CreatedAt);

    private sealed record TaskStatsRow(
        int TaskInProgressCount, int TaskUnderReviewCount, int TaskRevisionCount,
        int TaskCompletedCount, int TaskIncompleteCount,
        int TaskCancelledCount, int TaskDisputeCount);
}
