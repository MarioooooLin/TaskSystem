using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantWalletRepository : IMerchantWalletRepository
{
    public async Task<MerchantWallet?> GetByMerchantIdAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, MerchantId, AvailableAmount, FrozenAmount, TotalDepositedAmount, UpdatedAt
            FROM MerchantWallets WITH (UPDLOCK)
            WHERE MerchantId = @MerchantId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<MerchantWallet>(
            sql, new { MerchantId = merchantId }, session.Transaction);
    }

    public async Task<IReadOnlyList<MerchantWalletTransactionDto>> GetRecentTransactionsAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Take)
                t.Id,
                t.Type,
                t.Amount,
                t.Status,
                t.RelatedCaseId,
                c.Title     AS RelatedCaseTitle,
                t.Note,
                t.CreatedAt
            FROM MerchantWalletTransactions t
            LEFT JOIN Cases c ON c.Id = t.RelatedCaseId
            WHERE t.MerchantId = @MerchantId
            ORDER BY t.CreatedAt DESC
            """;

        var result = await session.Connection.QueryAsync<MerchantWalletTransactionDto>(
            sql, new { MerchantId = merchantId, Take = take }, session.Transaction);

        return result.AsList();
    }

    public async Task UpdateAsync(
        MerchantWallet wallet, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE MerchantWallets SET
                AvailableAmount         = @AvailableAmount,
                FrozenAmount            = @FrozenAmount,
                TotalDepositedAmount    = @TotalDepositedAmount,
                UpdatedAt               = GETUTCDATE()
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, wallet, session.Transaction);
    }
}
