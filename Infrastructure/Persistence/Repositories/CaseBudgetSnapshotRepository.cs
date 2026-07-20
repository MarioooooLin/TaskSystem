using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class CaseBudgetSnapshotRepository : ICaseBudgetSnapshotRepository
{
    public async Task<CaseBudgetSnapshot?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, RewardAmountPerKol, WantedKolCount, RewardSubtotal,
                   FeeItems, EstimatedFrozenAmount, SettingsSnapshot, IdempotencyKey, CreatedAt
            FROM CaseBudgetSnapshots
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseBudgetSnapshot>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<CaseBudgetSnapshot?> GetLatestByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP 1 Id, CaseId, RewardAmountPerKol, WantedKolCount, RewardSubtotal,
                         FeeItems, EstimatedFrozenAmount, SettingsSnapshot, IdempotencyKey, CreatedAt
            FROM CaseBudgetSnapshots
            WHERE CaseId = @CaseId
            ORDER BY CreatedAt DESC, Id DESC
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseBudgetSnapshot>(
            sql, new { CaseId = caseId }, session.Transaction);
    }

    public async Task<CaseBudgetSnapshot?> GetByIdempotencyKeyAsync(string idempotencyKey, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, RewardAmountPerKol, WantedKolCount, RewardSubtotal,
                   FeeItems, EstimatedFrozenAmount, SettingsSnapshot, IdempotencyKey, CreatedAt
            FROM CaseBudgetSnapshots
            WHERE IdempotencyKey = @IdempotencyKey
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseBudgetSnapshot>(
            sql, new { IdempotencyKey = idempotencyKey }, session.Transaction);
    }

    public async Task<long> InsertAsync(CaseBudgetSnapshot snapshot, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO CaseBudgetSnapshots (
                CaseId, RewardAmountPerKol, WantedKolCount, RewardSubtotal,
                FeeItems, EstimatedFrozenAmount, SettingsSnapshot, IdempotencyKey, CreatedAt
            ) VALUES (
                @CaseId, @RewardAmountPerKol, @WantedKolCount, @RewardSubtotal,
                @FeeItems, @EstimatedFrozenAmount, @SettingsSnapshot, @IdempotencyKey, GETUTCDATE()
            );
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql, snapshot, session.Transaction);
    }
}
