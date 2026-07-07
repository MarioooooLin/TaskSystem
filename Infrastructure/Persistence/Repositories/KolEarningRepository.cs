using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class KolEarningRepository : IKolEarningRepository
{
    public async Task<long> InsertAsync(
        KolEarning earning, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO KolEarnings
                (CaseId, TaskId, KolId, SourceType, GrossAmount, PlatformFeeAmount, NetAmount, Status, AvailableAt)
            VALUES
                (@CaseId, @TaskId, @KolId, @SourceType, @GrossAmount, @PlatformFeeAmount, @NetAmount, @Status, @AvailableAt);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(sql, earning, session.Transaction);
    }

    public async Task UpdateAsync(
        KolEarning earning, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE KolEarnings SET
                Status          = @Status,
                AvailableAt     = @AvailableAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, earning, session.Transaction);
    }

    public async Task<KolEarning?> GetByTaskIdAsync(
        long taskId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, TaskId, KolId, SourceType, GrossAmount,
                   PlatformFeeAmount, NetAmount, Status, AvailableAt, CreatedAt
            FROM KolEarnings
            WHERE TaskId = @TaskId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<KolEarning>(
            sql, new { TaskId = taskId }, session.Transaction);
    }
}
