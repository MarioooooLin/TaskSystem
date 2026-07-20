using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class FileRepository : IFileRepository
{
    public async Task<FileEntity?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, UploadedByUserId, FileName, FilePath, FileSize, MimeType, CreatedAt
            FROM Files
            WHERE Id = @Id
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<FileEntity>(
            sql, new { Id = id }, session.Transaction);
    }

    public async Task<long> InsertAsync(FileEntity file, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO Files (UploadedByUserId, FileName, FilePath, FileSize, MimeType)
            VALUES (@UploadedByUserId, @FileName, @FilePath, @FileSize, @MimeType);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql, file, session.Transaction);
    }

    public async Task DeleteAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM Files WHERE Id = @Id";
        await session.Connection.ExecuteAsync(sql, new { Id = id }, session.Transaction);
    }
}
