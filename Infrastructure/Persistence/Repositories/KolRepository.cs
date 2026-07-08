using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Common.Pagination;
using Dapper;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class KolRepository : IKolRepository
{
    // ── GetByIdAsync ──────────────────────────────────────────────
    public async Task<KolProfile?> GetByIdAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, UserId, DisplayName, RealName, Phone, LineContactId, Intro,
                   AcceptsCash, AcceptsBarter, AcceptsCommission, FollowersCount,
                   VerificationStatus, VerifiedAt, VerifiedByAdminId,
                   RejectionNote, SuspensionNote, CreatedAt, UpdatedAt
            FROM KolProfiles
            WHERE Id = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<KolProfile>(
            sql, new { KolId = kolId }, session.Transaction);
    }

    // ── GetListAsync ──────────────────────────────────────────────
    public async Task<(IReadOnlyList<KolListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        VerificationStatus? verificationStatus,
        IReadOnlyList<short>? categories,
        IReadOnlyList<short>? platforms,
        bool? hasBankAccount,
        DateTime? dateFrom,
        DateTime? dateTo,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default)
    {
        var conditions = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
            conditions.Add("(kp.DisplayName LIKE @Keyword OR u.Email LIKE @Keyword)");
        if (verificationStatus.HasValue)
            conditions.Add("kp.VerificationStatus = @Status");
        if (categories?.Count > 0)
            conditions.Add("EXISTS (SELECT 1 FROM KolCategories kc WHERE kc.KolId = kp.Id AND kc.Category IN @Categories)");
        if (platforms?.Count > 0)
            conditions.Add("EXISTS (SELECT 1 FROM KolSocialAccounts ksa WHERE ksa.KolId = kp.Id AND ksa.Platform IN @Platforms)");
        if (hasBankAccount.HasValue)
        {
            conditions.Add(hasBankAccount.Value
                ? "EXISTS (SELECT 1 FROM KolBankAccounts kba WHERE kba.KolId = kp.Id)"
                : "NOT EXISTS (SELECT 1 FROM KolBankAccounts kba WHERE kba.KolId = kp.Id)");
        }
        if (dateFrom.HasValue)
            conditions.Add("kp.CreatedAt >= @DateFrom");
        if (dateTo.HasValue)
            conditions.Add("kp.CreatedAt < @DateTo");

        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        var countSql = $"""
            SELECT COUNT(*)
            FROM KolProfiles kp
            JOIN Users u ON u.Id = kp.UserId
            {where}
            """;

        var dataSql = $"""
            SELECT
                kp.Id                   AS KolId,
                kp.UserId,
                kp.DisplayName,
                u.Email,
                kp.VerificationStatus,
                kp.CreatedAt,
                ISNULL(SUM(ksa.FollowersCount), 0) AS TotalFollowers,
                (SELECT COUNT(*) FROM Tasks t WHERE t.KolId = kp.Id) AS TaskCount,
                (SELECT COUNT(*) FROM Tasks t WHERE t.KolId = kp.Id AND t.Status = 6) AS CompletedTaskCount,
                (SELECT COUNT(*) FROM Disputes d JOIN Tasks t ON t.Id = d.TaskId WHERE t.KolId = kp.Id AND d.Status IN (1, 2)) AS DisputeCount,
                (SELECT TOP 1 kba.Status FROM KolBankAccounts kba WHERE kba.KolId = kp.Id ORDER BY kba.Id DESC) AS BankAccountStatus
            FROM KolProfiles kp
            JOIN Users u ON u.Id = kp.UserId
            LEFT JOIN KolSocialAccounts ksa ON ksa.KolId = kp.Id
            {where}
            GROUP BY kp.Id, kp.UserId, kp.DisplayName, u.Email, kp.VerificationStatus, kp.CreatedAt
            ORDER BY kp.CreatedAt DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = verificationStatus.HasValue ? (short)verificationStatus.Value : (short?)null,
            Categories = categories,
            Platforms = platforms,
            DateFrom = dateFrom,
            DateTo = dateTo.HasValue ? dateTo.Value.AddDays(1) : (DateTime?)null,
            Offset = page.Offset,
            PageSize = page.PageSize,
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        var rawItems = await session.Connection.QueryAsync<KolListItemDto>(
            dataSql, param, session.Transaction);

        var items = rawItems.AsList();

        // 載入 Categories / Platforms 子集合
        if (items.Count > 0)
        {
            var ids = items.Select(x => x.KolId).ToArray();

            var cats = await session.Connection.QueryAsync<(long KolId, short Category)>(
                "SELECT KolId, Category FROM KolCategories WHERE KolId IN @Ids",
                new { Ids = ids }, session.Transaction);

            var pls = await session.Connection.QueryAsync<(long KolId, short Platform)>(
                "SELECT DISTINCT KolId, Platform FROM KolSocialAccounts WHERE KolId IN @Ids",
                new { Ids = ids }, session.Transaction);

            var catMap = cats.GroupBy(x => x.KolId)
                             .ToDictionary(g => g.Key, g => (IReadOnlyList<short>)g.Select(x => x.Category).ToList());
            var plMap = pls.GroupBy(x => x.KolId)
                            .ToDictionary(g => g.Key, g => (IReadOnlyList<short>)g.Select(x => x.Platform).ToList());

            // KolListItemDto 用 init-only，需整批替換 — 透過轉型匿名投影
            items = items.Select(x => new KolListItemDto
            {
                KolId = x.KolId,
                UserId = x.UserId,
                DisplayName = x.DisplayName,
                Email = x.Email,
                Categories = catMap.TryGetValue(x.KolId, out var c) ? c : [],
                Platforms = plMap.TryGetValue(x.KolId, out var p) ? p : [],
                TotalFollowers = x.TotalFollowers,
                VerificationStatus = x.VerificationStatus,
                BankAccountStatus = x.BankAccountStatus,
                TaskCount = x.TaskCount,
                CompletedTaskCount = x.CompletedTaskCount,
                DisputeCount = x.DisputeCount,
                CreatedAt = x.CreatedAt,
            }).ToList();
        }

        return (items, totalCount);
    }

    // ── GetReviewListAsync ────────────────────────────────────────
    public async Task<(IReadOnlyList<KolReviewListItemDto> Items, int TotalCount)> GetReviewListAsync(
        string? keyword,
        string? statusFilter,
        short? category,
        short? platform,
        DateOnly? submittedDate,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default)
    {
        // statusFilter chip → SQL 條件
        var conditions = new List<string>();
        switch (statusFilter)
        {
            case "pending":
                conditions.Add("kp.VerificationStatus = 1");
                conditions.Add("NOT EXISTS (SELECT 1 FROM KolReviewEvents kre WHERE kre.KolId = kp.Id AND kre.ActionType = 2)");
                break;
            case "resubmit":
                conditions.Add("kp.VerificationStatus = 1");
                conditions.Add("EXISTS (SELECT 1 FROM KolReviewEvents kre WHERE kre.KolId = kp.Id AND kre.ActionType = 2)");
                break;
            case "returned":
                conditions.Add("kp.VerificationStatus = 3");
                break;
            case "approved":
                conditions.Add("kp.VerificationStatus = 2");
                break;
            default:
                // 預設：全部待審核（首次 + 重送）
                conditions.Add("kp.VerificationStatus = 1");
                break;
        }

        if (!string.IsNullOrWhiteSpace(keyword))
            conditions.Add("(kp.DisplayName LIKE @Keyword OR u.Email LIKE @Keyword)");
        if (category.HasValue)
            conditions.Add("EXISTS (SELECT 1 FROM KolCategories kc WHERE kc.KolId = kp.Id AND kc.Category = @Category)");
        if (platform.HasValue)
            conditions.Add("EXISTS (SELECT 1 FROM KolSocialAccounts ksa WHERE ksa.KolId = kp.Id AND ksa.Platform = @Platform)");
        if (submittedDate.HasValue)
            conditions.Add("CAST(kp.UpdatedAt AS DATE) = @SubmittedDate");

        var where = "WHERE " + string.Join(" AND ", conditions);

        var countSql = $"""
            SELECT COUNT(*)
            FROM KolProfiles kp
            JOIN Users u ON u.Id = kp.UserId
            {where}
            """;

        var dataSql = $"""
            SELECT
                kp.Id                   AS KolId,
                kp.DisplayName,
                u.Email,
                kp.VerificationStatus,
                kp.UpdatedAt            AS SubmittedAt,
                ISNULL(SUM(ksa.FollowersCount), 0) AS TotalFollowers,
                0                       AS ProfileCompleteness,
                CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM KolReviewEvents kre
                    WHERE kre.KolId = kp.Id AND kre.ActionType = 2
                ) THEN 1 ELSE 0 END AS BIT) AS IsResubmit
            FROM KolProfiles kp
            JOIN Users u ON u.Id = kp.UserId
            LEFT JOIN KolSocialAccounts ksa ON ksa.KolId = kp.Id
            {where}
            GROUP BY kp.Id, kp.DisplayName, u.Email, kp.VerificationStatus, kp.UpdatedAt
            ORDER BY kp.UpdatedAt ASC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Category = category,
            Platform = platform,
            SubmittedDate = submittedDate.HasValue ? (DateTime?)submittedDate.Value.ToDateTime(TimeOnly.MinValue) : null,
            Offset = page.Offset,
            PageSize = page.PageSize,
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(
            countSql, param, session.Transaction);

        var rawItems = await session.Connection.QueryAsync<KolReviewListItemDto>(
            dataSql, param, session.Transaction);

        var items = rawItems.AsList();

        // 載入 Categories / Platforms 子集合
        if (items.Count > 0)
        {
            var ids = items.Select(x => x.KolId).ToArray();

            var cats = await session.Connection.QueryAsync<(long KolId, short Category)>(
                "SELECT KolId, Category FROM KolCategories WHERE KolId IN @Ids",
                new { Ids = ids }, session.Transaction);

            var pls = await session.Connection.QueryAsync<(long KolId, short Platform)>(
                "SELECT DISTINCT KolId, Platform FROM KolSocialAccounts WHERE KolId IN @Ids",
                new { Ids = ids }, session.Transaction);

            var catMap = cats.GroupBy(x => x.KolId)
                             .ToDictionary(g => g.Key, g => (IReadOnlyList<short>)g.Select(x => x.Category).ToList());
            var plMap = pls.GroupBy(x => x.KolId)
                            .ToDictionary(g => g.Key, g => (IReadOnlyList<short>)g.Select(x => x.Platform).ToList());

            items = items.Select(x => new KolReviewListItemDto
            {
                KolId = x.KolId,
                DisplayName = x.DisplayName,
                Email = x.Email,
                Categories = catMap.TryGetValue(x.KolId, out var c) ? c : [],
                Platforms = plMap.TryGetValue(x.KolId, out var p) ? p : [],
                TotalFollowers = x.TotalFollowers,
                ProfileCompleteness = x.ProfileCompleteness,
                IsResubmit = x.IsResubmit,
                VerificationStatus = x.VerificationStatus,
                SubmittedAt = x.SubmittedAt,
            }).ToList();
        }

        return (items, totalCount);
    }

    // ── GetDetailBaseAsync ────────────────────────────────────────
    public async Task<KolDetailBaseDto?> GetDetailBaseAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                kp.Id               AS KolId,
                kp.UserId,
                kp.DisplayName,
                kp.RealName,
                u.Email             AS UserEmail,
                kp.Phone,
                kp.LineContactId,
                kp.Intro,
                kp.AcceptsCash,
                kp.AcceptsBarter,
                kp.AcceptsCommission,
                kp.FollowersCount,
                kp.VerificationStatus,
                kp.VerifiedAt,
                adm.Name            AS VerifiedByAdminName,
                kp.RejectionNote,
                kp.SuspensionNote,
                kp.CreatedAt,
                kp.UpdatedAt
            FROM KolProfiles kp
            JOIN Users u ON u.Id = kp.UserId
            LEFT JOIN Users adm ON adm.Id = kp.VerifiedByAdminId
            WHERE kp.Id = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<KolDetailBaseDto>(
            sql, new { KolId = kolId }, session.Transaction);
    }

    // ── UpdateAsync ───────────────────────────────────────────────
    public async Task UpdateAsync(
        KolProfile kol, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE KolProfiles SET
                VerificationStatus  = @VerificationStatus,
                VerifiedAt          = @VerifiedAt,
                VerifiedByAdminId   = @VerifiedByAdminId,
                RejectionNote       = @RejectionNote,
                SuspensionNote      = @SuspensionNote,
                UpdatedAt           = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, kol, session.Transaction);
    }

    // ── GetReviewSummaryAsync ─────────────────────────────────────
    public async Task<KolReviewSummaryDto> GetReviewSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                SUM(CASE WHEN kp.VerificationStatus = 1 AND kre.KolId IS NULL     THEN 1 ELSE 0 END) AS PendingCount,
                SUM(CASE WHEN kp.VerificationStatus = 1 AND kre.KolId IS NOT NULL  THEN 1 ELSE 0 END) AS ResubmitCount,
                SUM(CASE WHEN kp.VerificationStatus = 3                             THEN 1 ELSE 0 END) AS ReturnedCount,
                SUM(CASE
                    WHEN kp.VerificationStatus IN (1, 3)
                         AND CAST(kp.UpdatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)
                    THEN 1 ELSE 0 END) AS TodayNewCount,
                SUM(CASE
                    WHEN kp.VerificationStatus = 1
                         AND kp.UpdatedAt < DATEADD(day, -3, GETUTCDATE())
                    THEN 1 ELSE 0 END) AS OverdueCount
            FROM KolProfiles kp
            LEFT JOIN (
                SELECT DISTINCT KolId
                FROM KolReviewEvents
                WHERE ActionType = 2
            ) kre ON kre.KolId = kp.Id
            """;

        return await session.Connection.QueryFirstAsync<KolReviewSummaryDto>(
            sql, transaction: session.Transaction);
    }

    // ── GetSummaryAsync ───────────────────────────────────────────
    public async Task<KolSummaryDto> GetSummaryAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                COUNT(*)                                                            AS TotalCount,
                SUM(CASE WHEN kp.VerificationStatus = 2 THEN 1 ELSE 0 END)        AS ActiveCount,
                SUM(CASE WHEN kp.VerificationStatus = 1 THEN 1 ELSE 0 END)        AS PendingCount,
                SUM(CASE WHEN kp.VerificationStatus = 3 THEN 1 ELSE 0 END)        AS RejectedCount,
                SUM(CASE WHEN kp.VerificationStatus = 4 THEN 1 ELSE 0 END)        AS SuspendedCount,
                (SELECT COUNT(DISTINCT t.KolId)
                 FROM Disputes d
                 JOIN Tasks t ON t.Id = d.TaskId
                 WHERE d.Status IN (1, 2) AND t.KolId IS NOT NULL)                 AS AbnormalCount
            FROM KolProfiles kp
            """;

        return await session.Connection.QueryFirstAsync<KolSummaryDto>(
            sql, transaction: session.Transaction);
    }
}
