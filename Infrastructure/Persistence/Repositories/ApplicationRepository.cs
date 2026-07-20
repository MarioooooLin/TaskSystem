using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class ApplicationRepository : IApplicationRepository
{
    public async Task<Domain.Entities.Application?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, KolId, Status, Message, IsRequirementMatched,
                   MismatchReasons, ReconfirmedAt, AppliedAt, ReviewedAt, ReviewedByUserId
            FROM CaseApplications
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Domain.Entities.Application>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<Domain.Entities.Application?> GetByCaseAndKolAsync(long caseId, long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, KolId, Status, Message, IsRequirementMatched,
                   MismatchReasons, ReconfirmedAt, AppliedAt, ReviewedAt, ReviewedByUserId
            FROM CaseApplications
            WHERE CaseId = @CaseId AND KolId = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<Domain.Entities.Application>(
            sql, new { CaseId = caseId, KolId = kolId }, session.Transaction);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, CaseId, KolId, Status, Message, IsRequirementMatched,
                   MismatchReasons, ReconfirmedAt, AppliedAt, ReviewedAt, ReviewedByUserId
            FROM CaseApplications
            WHERE CaseId = @CaseId
            ORDER BY AppliedAt DESC
            """;

        var result = await session.Connection.QueryAsync<Domain.Entities.Application>(
            sql, new { CaseId = caseId }, session.Transaction);
        return result.AsList();
    }

    public async Task<int> CountAcceptedAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM CaseApplications
            WHERE CaseId = @CaseId AND Status = 2
            """;

        return await session.Connection.ExecuteScalarAsync<int>(
            sql, new { CaseId = caseId }, session.Transaction);
    }

    public async Task<long> InsertAsync(Domain.Entities.Application application, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO CaseApplications (
                CaseId, KolId, Status, Message, IsRequirementMatched,
                MismatchReasons, ReconfirmedAt, AppliedAt, ReviewedAt, ReviewedByUserId
            ) VALUES (
                @CaseId, @KolId, @Status, @Message, @IsRequirementMatched,
                @MismatchReasons, @ReconfirmedAt, GETUTCDATE(), @ReviewedAt, @ReviewedByUserId
            );
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql, application, session.Transaction);
    }

    public async Task UpdateAsync(Domain.Entities.Application application, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE CaseApplications SET
                CaseId = @CaseId,
                KolId = @KolId,
                Status = @Status,
                Message = @Message,
                IsRequirementMatched = @IsRequirementMatched,
                MismatchReasons = @MismatchReasons,
                ReconfirmedAt = @ReconfirmedAt,
                AppliedAt = @AppliedAt,
                ReviewedAt = @ReviewedAt,
                ReviewedByUserId = @ReviewedByUserId
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, application, session.Transaction);
    }

    public async Task UpdateManyAsync(IEnumerable<Domain.Entities.Application> applications, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE CaseApplications SET
                CaseId = @CaseId,
                KolId = @KolId,
                Status = @Status,
                Message = @Message,
                IsRequirementMatched = @IsRequirementMatched,
                MismatchReasons = @MismatchReasons,
                ReconfirmedAt = @ReconfirmedAt,
                AppliedAt = @AppliedAt,
                ReviewedAt = @ReviewedAt,
                ReviewedByUserId = @ReviewedByUserId
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(sql, applications, session.Transaction);
    }

    public async Task SetPendingReconfirmationAsync(long caseId, DateTime deadlineAt, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE CaseApplications
            SET Status = 3,
                ReviewedAt = NULL,
                ReviewedByUserId = NULL
            WHERE CaseId = @CaseId AND Status = 2
            """;

        await session.Connection.ExecuteAsync(
            sql, new { CaseId = caseId }, session.Transaction);
    }
}
