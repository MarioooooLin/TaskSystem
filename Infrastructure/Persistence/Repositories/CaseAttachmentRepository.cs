using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class CaseAttachmentRepository : ICaseAttachmentRepository
{
    public async Task<CaseAttachment?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ca.Id, ca.CaseId, ca.FileId, ca.Type, ca.CreatedAt,
                f.Id, f.UploadedByUserId, f.FileName, f.FilePath, f.FileSize, f.MimeType, f.CreatedAt
            FROM CaseAttachments ca
            INNER JOIN Files f ON f.Id = ca.FileId
            WHERE ca.Id = @Id
            """;

        return (await session.Connection.QueryAsync<CaseAttachment, FileEntity, CaseAttachment>(
            sql,
            (ca, f) =>
            {
                ca.File = f;
                return ca;
            },
            new { Id = id },
            session.Transaction)).FirstOrDefault();
    }

    public async Task<IReadOnlyList<CaseAttachment>> GetByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                ca.Id, ca.CaseId, ca.FileId, ca.Type, ca.CreatedAt,
                f.Id, f.UploadedByUserId, f.FileName, f.FilePath, f.FileSize, f.MimeType, f.CreatedAt
            FROM CaseAttachments ca
            INNER JOIN Files f ON f.Id = ca.FileId
            WHERE ca.CaseId = @CaseId
            ORDER BY ca.CreatedAt DESC
            """;

        return (await session.Connection.QueryAsync<CaseAttachment, FileEntity, CaseAttachment>(
            sql,
            (ca, f) =>
            {
                ca.File = f;
                return ca;
            },
            new { CaseId = caseId },
            session.Transaction)).AsList();
    }

    public async Task<int> CountByCaseAndTypeAsync(long caseId, short type, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM CaseAttachments
            WHERE CaseId = @CaseId AND Type = @Type
            """;

        return await session.Connection.ExecuteScalarAsync<int>(
            sql, new { CaseId = caseId, Type = type }, session.Transaction);
    }

    public async Task<int> CountTotalSizeByCaseAsync(long caseId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT ISNULL(SUM(f.FileSize), 0)
            FROM CaseAttachments ca
            INNER JOIN Files f ON f.Id = ca.FileId
            WHERE ca.CaseId = @CaseId
            """;

        return await session.Connection.ExecuteScalarAsync<int>(
            sql, new { CaseId = caseId }, session.Transaction);
    }

    public async Task<long> InsertAsync(CaseAttachment attachment, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO CaseAttachments (CaseId, FileId, Type)
            VALUES (@CaseId, @FileId, @Type);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql, attachment, session.Transaction);
    }

    public async Task DeleteAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM CaseAttachments WHERE Id = @Id";
        await session.Connection.ExecuteAsync(sql, new { Id = id }, session.Transaction);
    }
}
