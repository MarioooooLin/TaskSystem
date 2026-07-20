using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantCreditWalletRepository : IMerchantCreditWalletRepository
{
    // ── GetByMerchantIdAsync ──────────────────────────────────────
    public async Task<MerchantCreditWallet?> GetByMerchantIdAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, MerchantId, AvailableAmount, UsedAmount, ExpiredAmount, RevokedAmount, UpdatedAt
            FROM MerchantCreditWallets WITH (UPDLOCK)
            WHERE MerchantId = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantCreditWallet>(
            sql, new { MerchantId = merchantId }, session.Transaction);
    }

    // ── GetSummaryAsync ───────────────────────────────────────────
    public async Task<MerchantCreditWalletSummaryDto> GetSummaryAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ISNULL(w.AvailableAmount, 0)  AS AvailableAmount,
                ISNULL(
                    (SELECT SUM(ABS(t.Amount))
                     FROM MerchantCreditTransactions t
                     WHERE t.MerchantId = @MerchantId
                       AND t.Type = 2
                       AND t.Status = 2
                       AND t.CreatedAt >= DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1)),
                    0) AS MonthlyUsedAmount
            FROM MerchantCreditWallets w
            WHERE w.MerchantId = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantCreditWalletSummaryDto>(
            sql, new { MerchantId = merchantId }, session.Transaction)
            ?? new MerchantCreditWalletSummaryDto();
    }

    // ── GetRecentGrantsAsync ──────────────────────────────────────
    public async Task<IReadOnlyList<MerchantCreditTransactionDto>> GetRecentGrantsAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                t.Id,
                t.Type,
                t.Amount,
                t.RelatedCaseId,
                t.Reason,
                t.Note,
                u.Name  AS CreatedByName,
                t.CreatedAt
            FROM MerchantCreditTransactions t
            LEFT JOIN Users u ON u.Id = t.CreatedByUserId
            WHERE t.MerchantId = @MerchantId
              AND t.Type IN (1, 4, 6)
            ORDER BY t.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<MerchantCreditTransactionDto>(
            sql, new { MerchantId = merchantId, Take = take }, session.Transaction);

        return result.AsList();
    }

    // ── GetRecentUsagesAsync ──────────────────────────────────────
    public async Task<IReadOnlyList<MerchantCreditTransactionDto>> GetRecentUsagesAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                t.Id,
                t.Type,
                t.Amount,
                t.RelatedCaseId,
                t.Reason,
                t.Note,
                u.Name  AS CreatedByName,
                t.CreatedAt
            FROM MerchantCreditTransactions t
            LEFT JOIN Users u ON u.Id = t.CreatedByUserId
            WHERE t.MerchantId = @MerchantId
              AND t.Type = 2
            ORDER BY t.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<MerchantCreditTransactionDto>(
            sql, new { MerchantId = merchantId, Take = take }, session.Transaction);

        return result.AsList();
    }

    // ── UpsertAsync ───────────────────────────────────────────────
    public async Task UpsertAsync(
        MerchantCreditWallet wallet, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            MERGE MerchantCreditWallets AS target
            USING (SELECT @MerchantId AS MerchantId) AS source ON target.MerchantId = source.MerchantId
            WHEN MATCHED THEN
                UPDATE SET
                    AvailableAmount = @AvailableAmount,
                    UsedAmount      = @UsedAmount,
                    RevokedAmount   = @RevokedAmount,
                    UpdatedAt       = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (MerchantId, AvailableAmount, UsedAmount, ExpiredAmount, RevokedAmount)
                VALUES (@MerchantId, @AvailableAmount, @UsedAmount, 0, @RevokedAmount);
            """;

        await session.Connection.ExecuteAsync(sql, wallet, session.Transaction);
    }

    // ── UpdateAsync ───────────────────────────────────────────────
    public async Task UpdateAsync(
        MerchantCreditWallet wallet, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE MerchantCreditWallets SET
                AvailableAmount = @AvailableAmount,
                UsedAmount      = @UsedAmount,
                ExpiredAmount   = @ExpiredAmount,
                RevokedAmount   = @RevokedAmount,
                UpdatedAt       = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, wallet, session.Transaction);
    }

    // ── InsertTransactionAsync ────────────────────────────────────
    public async Task InsertTransactionAsync(
        long merchantId, short type, decimal amount,
        long? relatedCaseId, string? reason, string? note,
        long? createdByUserId,
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO MerchantCreditTransactions
                (MerchantId, Type, Amount, Status, RelatedCaseId, Reason, Note, CreatedByUserId)
            VALUES
                (@MerchantId, @Type, @Amount, 2, @RelatedCaseId, @Reason, @Note, @CreatedByUserId)
            """;

        await session.Connection.ExecuteAsync(sql,
            new
            {
                MerchantId = merchantId,
                Type = type,
                Amount = amount,
                RelatedCaseId = relatedCaseId,
                Reason = reason,
                Note = note,
                CreatedByUserId = createdByUserId
            },
            session.Transaction);
    }
}
