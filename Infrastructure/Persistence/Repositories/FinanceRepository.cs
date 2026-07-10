using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Finance.DTOs;
using Common.Pagination;
using Dapper;
using Domain.Enums;

namespace Infrastructure.Persistence.Repositories;

public sealed class FinanceRepository : IFinanceRepository
{
    // Type: 1=OfflineDeposit 2=TaskBudgetFreeze 3=TaskBudgetRelease 4=TaskBudgetSettle 5=DisputeHold 6=ManualAdjustment
    // Status: 1=Pending 2=Approved 3=Rejected 4=Completed 5=Cancelled
    private const short TxTypeOfflineDeposit = 1;
    private const short TxTypeTaskBudgetRelease = 3;
    private const short TxTypeTaskBudgetSettle = 4;
    private const short TxTypeManualAdjustment = 6;
    private const short TxStatusCompleted = 4;

    // Credit Type: 1=Grant 2=Use 3=Refund 4=Revoke 5=Expire 6=ManualAdjustment
    // Credit Status: 1=Pending 2=Completed 3=Cancelled
    private const short CreditTypeUse = 2;
    private const short CreditStatusCompleted = 2;

    public async Task<FinanceSummaryDto> GetSummaryAsync(
        DateTime dateFrom,
        DateTime dateTo,
        IDbSession session,
        CancellationToken ct = default)
    {
        // 平台總收入：已完成案件結算 + 正向人工調整 - 折扣金折抵開案費
        const string incomeSql = """
            SELECT
                ISNULL(SUM(CASE WHEN Type = @SettleType AND Status = @CompletedStatus THEN Amount ELSE 0 END), 0)
                + ISNULL(SUM(CASE WHEN Type = @ManualType AND Amount > 0 THEN Amount ELSE 0 END), 0)
                - ISNULL((
                    SELECT SUM(Amount)
                    FROM MerchantCreditTransactions
                    WHERE Type = @CreditUseType
                      AND Status = @CreditCompletedStatus
                      AND CreatedAt BETWEEN @DateFrom AND @DateTo
                  ), 0) AS TotalIncome
            FROM MerchantWalletTransactions
            WHERE CreatedAt BETWEEN @DateFrom AND @DateTo
            """;

        // 平台總支出：KOL 淨付金額 + 負向人工調整 + 退款/釋放
        const string expenseSql = """
            SELECT
                ISNULL(SUM(CASE WHEN Status <> @CancelledStatus THEN NetAmount ELSE 0 END), 0)
                + ISNULL((
                    SELECT SUM(ABS(Amount))
                    FROM MerchantWalletTransactions
                    WHERE Type = @ManualType
                      AND Amount < 0
                      AND Status = @CompletedStatus
                      AND CreatedAt BETWEEN @DateFrom AND @DateTo
                  ), 0)
                + ISNULL((
                    SELECT SUM(Amount)
                    FROM MerchantWalletTransactions
                    WHERE Type = @ReleaseType
                      AND Status = @CompletedStatus
                      AND CreatedAt BETWEEN @DateFrom AND @DateTo
                  ), 0) AS TotalExpense
            FROM KolEarnings
            WHERE CreatedAt BETWEEN @DateFrom AND @DateTo
            """;

        // 資金流入：業者線下儲值
        const string fundInflowSql = """
            SELECT ISNULL(SUM(Amount), 0) AS FundInflow
            FROM MerchantWalletTransactions
            WHERE Type = @OfflineDepositType
              AND Status IN (@ApprovedStatus, @CompletedStatus)
              AND CreatedAt BETWEEN @DateFrom AND @DateTo
            """;

        var param = new
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            SettleType = TxTypeTaskBudgetSettle,
            ReleaseType = TxTypeTaskBudgetRelease,
            ManualType = TxTypeManualAdjustment,
            OfflineDepositType = TxTypeOfflineDeposit,
            CompletedStatus = TxStatusCompleted,
            ApprovedStatus = (short)2,
            CancelledStatus = (short)6,
            CreditUseType = CreditTypeUse,
            CreditCompletedStatus = CreditStatusCompleted
        };

        var income = await session.Connection.ExecuteScalarAsync<decimal>(incomeSql, param, session.Transaction);
        var expense = await session.Connection.ExecuteScalarAsync<decimal>(expenseSql, param, session.Transaction);
        var fundInflow = await session.Connection.ExecuteScalarAsync<decimal>(fundInflowSql, param, session.Transaction);

        return new FinanceSummaryDto
        {
            TotalIncome = income,
            TotalExpense = expense,
            GrossProfit = income - expense,
            FundInflow = fundInflow
        };
    }

    public async Task<(IReadOnlyList<FinanceListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        CaseStatus? status,
        DateTime dateFrom,
        DateTime dateTo,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default)
    {
        var where = BuildListWhere(keyword, status, dateFrom, dateTo);

        var countSql = $"""
            SELECT COUNT(DISTINCT c.Id)
            FROM Cases c
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            {where}
            """;

        var listSql = $"""
            SELECT
                c.Id                                                                        AS CaseId,
                'CASE-' + FORMAT(c.CreatedAt, 'yyyyMM') + '-' + CAST(c.Id AS VARCHAR(20))   AS CaseNo,
                c.Title,
                m.Id                                                                        AS MerchantId,
                m.CompanyName                                                               AS MerchantName,
                c.Status,
                c.SettledAt,
                ISNULL(mws.SettleAmount, 0)                                                 AS IncomeAmount,
                ISNULL(ke.KolExpense, 0)                                                    AS ExpenseAmount,
                CASE WHEN d.CaseId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END  AS HasOpenDispute,
                c.IsCommissionEnabled
            FROM Cases c
            INNER JOIN Merchants m ON m.Id = c.MerchantId
            LEFT JOIN (
                SELECT RelatedCaseId, SUM(Amount) AS SettleAmount
                FROM MerchantWalletTransactions
                WHERE Type = @SettleType AND Status = @CompletedStatus
                GROUP BY RelatedCaseId
            ) mws ON mws.RelatedCaseId = c.Id
            LEFT JOIN (
                SELECT CaseId, SUM(NetAmount) AS KolExpense
                FROM KolEarnings
                WHERE Status <> @CancelledStatus
                GROUP BY CaseId
            ) ke ON ke.CaseId = c.Id
            LEFT JOIN (
                SELECT DISTINCT CaseId FROM Disputes WHERE Status IN (1, 2)
            ) d ON d.CaseId = c.Id
            {where}
            ORDER BY COALESCE(c.SettledAt, c.CreatedAt) DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            Keyword = string.IsNullOrWhiteSpace(keyword) ? null : $"%{keyword.Trim()}%",
            Status = (short?)status,
            DateFrom = dateFrom,
            DateTo = dateTo,
            SettleType = TxTypeTaskBudgetSettle,
            CompletedStatus = TxStatusCompleted,
            CancelledStatus = (short)6,
            pageQuery.Offset,
            pageQuery.PageSize
        };

        var totalCount = await session.Connection.ExecuteScalarAsync<int>(countSql, param, session.Transaction);
        var items = (await session.Connection.QueryAsync<FinanceListItemDto>(listSql, param, session.Transaction)).AsList();

        if (items.Count > 0)
        {
            var caseIds = items.Select(x => x.CaseId).ToList();
            var details = await GetTaskDetailsAsync(caseIds, session, ct);
            items = items.Select(i => new FinanceListItemDto
            {
                CaseId = i.CaseId,
                CaseNo = i.CaseNo,
                Title = i.Title,
                MerchantId = i.MerchantId,
                MerchantName = i.MerchantName,
                Status = i.Status,
                SettledAt = i.SettledAt,
                IncomeAmount = i.IncomeAmount,
                ExpenseAmount = i.ExpenseAmount,
                HasOpenDispute = i.HasOpenDispute,
                IsCommissionEnabled = i.IsCommissionEnabled,
                TaskDetails = details.GetValueOrDefault(i.CaseId, Array.Empty<FinanceTaskDetailDto>())
            }).ToList();
        }

        return (items, totalCount);
    }

    private static async Task<Dictionary<long, IReadOnlyList<FinanceTaskDetailDto>>> GetTaskDetailsAsync(
        IReadOnlyList<long> caseIds,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                t.CaseId        AS CaseId,
                t.Id            AS TaskId,
                t.KolId,
                kp.DisplayName  AS KolName,
                ISNULL(SUM(CASE WHEN ke.SourceType = @CashSource THEN ke.GrossAmount ELSE 0 END), 0) AS MissionAmount,
                ISNULL(SUM(CASE WHEN ke.SourceType = @CommissionSource THEN ke.GrossAmount ELSE 0 END), 0) AS AffiliateAmount,
                ISNULL(SUM(ke.PlatformFeeAmount), 0) AS TaxFeeAmount,
                ISNULL(SUM(ke.NetAmount), 0) AS NetAmount,
                CASE MIN(ke.Status)
                    WHEN 1 THEN '待結算'
                    WHEN 2 THEN '保留'
                    WHEN 3 THEN '可請款'
                    WHEN 4 THEN '已申請提領'
                    WHEN 5 THEN '已撥款'
                    WHEN 6 THEN '已取消'
                    ELSE '—'
                END AS StatusText
            FROM Tasks t
            INNER JOIN KolProfiles kp ON kp.Id = t.KolId
            LEFT JOIN KolEarnings ke ON ke.TaskId = t.Id AND ke.Status <> @CancelledStatus
            WHERE t.CaseId IN @CaseIds
            GROUP BY t.CaseId, t.Id, t.KolId, kp.DisplayName
            ORDER BY t.Id
            """;

        var rows = await session.Connection.QueryAsync<FinanceTaskDetailDto>(
            sql,
            new
            {
                CaseIds = caseIds,
                CashSource = (short)1,
                CommissionSource = (short)2,
                CancelledStatus = (short)6
            },
            session.Transaction);

        return rows
            .GroupBy(r => r.CaseId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<FinanceTaskDetailDto>)g.ToList().AsReadOnly());
    }

    private static string BuildListWhere(string? keyword, CaseStatus? status, DateTime dateFrom, DateTime dateTo)
    {
        var clauses = new List<string>
        {
            // 時間範圍：以案件建立日期為主（待 PM 確認是否改為結算日期）
            "c.CreatedAt BETWEEN @DateFrom AND @DateTo"
        };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            clauses.Add("""
                (
                    c.Title LIKE @Keyword
                    OR m.CompanyName LIKE @Keyword
                    OR CAST(c.Id AS VARCHAR(20)) LIKE @Keyword
                )
                """);
        }

        if (status.HasValue)
        {
            clauses.Add("c.Status = @Status");
        }

        return "WHERE " + string.Join(" AND ", clauses);
    }
}
