using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    public async Task<CaseTask?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, KolId, ApplicationId, Status, CancellationSource,
                   StartedAt, SubmittedAt, CompletedAt, CancelledAt
            FROM Tasks
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseTask>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<CaseTask?> GetFirstPendingMatchAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP 1 Id, CaseId, KolId, ApplicationId, Status, CancellationSource,
                         StartedAt, SubmittedAt, CompletedAt, CancelledAt
            FROM Tasks WITH (UPDLOCK)
            WHERE CaseId = @CaseId AND Status = 1 AND KolId IS NULL AND ApplicationId IS NULL
            ORDER BY Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<CaseTask>(
            sql, new { CaseId = caseId }, session.Transaction);
    }

    public async Task<IReadOnlyList<CaseTask>> GetByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, KolId, ApplicationId, Status, CancellationSource,
                   StartedAt, SubmittedAt, CompletedAt, CancelledAt
            FROM Tasks
            WHERE CaseId = @CaseId
            ORDER BY Id
            """;

        var result = await session.Connection.QueryAsync<CaseTask>(
            sql, new { CaseId = caseId }, session.Transaction);
        return result.AsList();
    }

    public async Task InsertManyAsync(IEnumerable<CaseTask> tasks, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Tasks (CaseId, KolId, ApplicationId, Status, CancellationSource,
                               StartedAt, SubmittedAt, CompletedAt, CancelledAt)
            VALUES (@CaseId, @KolId, @ApplicationId, @Status, @CancellationSource,
                    @StartedAt, @SubmittedAt, @CompletedAt, @CancelledAt)
            """;

        await session.Connection.ExecuteAsync(sql, tasks, session.Transaction);
    }

    public async Task UpdateAsync(CaseTask task, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Tasks SET
                CaseId = @CaseId,
                KolId = @KolId,
                ApplicationId = @ApplicationId,
                Status = @Status,
                CancellationSource = @CancellationSource,
                StartedAt = @StartedAt,
                SubmittedAt = @SubmittedAt,
                CompletedAt = @CompletedAt,
                CancelledAt = @CancelledAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, task, session.Transaction);
    }

    public async Task UpdateManyAsync(IEnumerable<CaseTask> tasks, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE Tasks SET
                CaseId = @CaseId,
                KolId = @KolId,
                ApplicationId = @ApplicationId,
                Status = @Status,
                CancellationSource = @CancellationSource,
                StartedAt = @StartedAt,
                SubmittedAt = @SubmittedAt,
                CompletedAt = @CompletedAt,
                CancelledAt = @CancelledAt
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, tasks, session.Transaction);
    }

    public async Task<int> CountPendingMatchAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM Tasks
            WHERE CaseId = @CaseId AND Status = 1 AND KolId IS NULL AND ApplicationId IS NULL
            """;

        return await session.Connection.ExecuteScalarAsync<int>(
            sql, new { CaseId = caseId }, session.Transaction);
    }

    public async Task DeleteUnboundPendingMatchAsync(long caseId, int count, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM Tasks
            WHERE Id IN (
                SELECT TOP (@Count) Id
                FROM Tasks
                WHERE CaseId = @CaseId AND Status = 1 AND KolId IS NULL AND ApplicationId IS NULL
                ORDER BY Id DESC
            )
            """;

        await session.Connection.ExecuteAsync(
            sql, new { CaseId = caseId, Count = count }, session.Transaction);
    }

    public async Task<int> CountBoundAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM Tasks
            WHERE CaseId = @CaseId AND KolId IS NOT NULL
            """;

        return await session.Connection.ExecuteScalarAsync<int>(
            sql, new { CaseId = caseId }, session.Transaction);
    }
}
