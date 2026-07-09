using System.Data;
using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Dashboard.DTOs;
using Application.Kols.DTOs;
using Dapper;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class DashboardRepository : IDashboardRepository
{
    public async Task<DashboardDto> GetDashboardAsync(
        int topK, IDbSession session, CancellationToken ct = default)
    {
        var conn = session.Connection;
        var tx = session.Transaction;

        // 1. KPI 統計
        const string kpiSql = """
            SELECT
                (SELECT COUNT(*) FROM Merchants WHERE VerificationStatus = 2)               AS ActiveMerchantCount,
                (SELECT COUNT(*) FROM KolProfiles WHERE VerificationStatus = 2)              AS ActiveKolCount,
                (SELECT COUNT(*) FROM Cases WHERE Status = 4)                                AS InProgressCaseCount,
                (SELECT COUNT(*) FROM Disputes WHERE Status IN (1, 2))                       AS DisputeCount,
                ISNULL((SELECT SUM(NetAmount) FROM KolEarnings WHERE Status IN (2, 4)), 0)   AS PendingPayoutAmount
            """;
        var kpi = await conn.QueryFirstOrDefaultAsync<DashboardKpiDto>(kpiSql, transaction: tx) ?? new DashboardKpiDto();

        // 2. 時效異常監控
        const string alertSql = """
            SELECT
                (SELECT COUNT(DISTINCT c.Id)
                 FROM Cases c
                 WHERE c.SubmissionDeadline < GETUTCDATE()
                   AND c.Status IN (2, 3, 4))                                               AS OverdueCaseCount,
                (SELECT COUNT(*)
                 FROM Submissions s
                 WHERE s.Status = 1
                   AND s.ReviewDeadlineAt < GETUTCDATE())                                   AS ReviewOverdueCount,
                0                                                                            AS AffiliateSyncErrorCount
            """;
        var alerts = await conn.QueryFirstOrDefaultAsync<DashboardAlertDto>(alertSql, transaction: tx) ?? new DashboardAlertDto();

        // 3. KOL 審核待辦（最新 topK 筆）
        var pendingKols = await GetPendingKolReviewsAsync(topK, conn, tx);

        // 4. 最近異議（最新 topK 筆）
        var disputes = await GetRecentDisputesAsync(topK, conn, tx);

        return new DashboardDto
        {
            Kpi = kpi,
            Alerts = alerts,
            PendingKolReviews = pendingKols,
            RecentDisputes = disputes,
        };
    }

    private static async Task<IReadOnlyList<KolReviewListItemDto>> GetPendingKolReviewsAsync(
        int topK, IDbConnection conn, IDbTransaction? tx)
    {
        const string sql = """
            SELECT TOP (@TopK)
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
            WHERE kp.VerificationStatus = 1
            GROUP BY kp.Id, kp.DisplayName, u.Email, kp.VerificationStatus, kp.UpdatedAt
            ORDER BY kp.UpdatedAt ASC
            """;

        var rawItems = (await conn.QueryAsync<KolReviewListItemDto>(sql, new { TopK = topK }, tx)).AsList();
        if (rawItems.Count == 0) return rawItems;

        var ids = rawItems.Select(x => x.KolId).ToArray();

        var cats = await conn.QueryAsync<(long KolId, short Category)>(
            "SELECT KolId, Category FROM KolCategories WHERE KolId IN @Ids",
            new { Ids = ids }, tx);

        var pls = await conn.QueryAsync<(long KolId, short Platform)>(
            "SELECT DISTINCT KolId, Platform FROM KolSocialAccounts WHERE KolId IN @Ids",
            new { Ids = ids }, tx);

        var catMap = cats.GroupBy(x => x.KolId)
                         .ToDictionary(g => g.Key, g => (IReadOnlyList<short>)g.Select(x => x.Category).ToList());
        var plMap = pls.GroupBy(x => x.KolId)
                        .ToDictionary(g => g.Key, g => (IReadOnlyList<short>)g.Select(x => x.Platform).ToList());

        return rawItems.Select(x => new KolReviewListItemDto
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

    private static async Task<IReadOnlyList<DashboardDisputeItemDto>> GetRecentDisputesAsync(
        int topK, IDbConnection conn, IDbTransaction? tx)
    {
        const string sql = """
            SELECT TOP (@TopK)
                d.Id        AS DisputeId,
                d.CaseId,
                c.Title     AS CaseTitle,
                m.CompanyName AS MerchantName,
                d.Reason    AS DisputeType,
                d.Status,
                d.OpenedAt
            FROM Disputes d
            JOIN Cases c ON c.Id = d.CaseId
            JOIN Merchants m ON m.Id = c.MerchantId
            WHERE d.Status IN (1, 2)
            ORDER BY d.OpenedAt DESC
            """;

        var items = (await conn.QueryAsync<DashboardDisputeItemDto>(sql, new { TopK = topK }, tx)).AsList();

        return items.Select(x => new DashboardDisputeItemDto
        {
            DisputeId = x.DisputeId,
            DisputeNo = $"DSP-{x.OpenedAt:yyyyMM}-{x.DisputeId:D3}",
            CaseId = x.CaseId,
            CaseTitle = x.CaseTitle,
            MerchantName = x.MerchantName,
            DisputeType = string.IsNullOrWhiteSpace(x.DisputeType) ? "—" : x.DisputeType,
            Status = x.Status,
            OpenedAt = x.OpenedAt,
        }).ToList();
    }
}
