using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class MerchantContactRepository : IMerchantContactRepository
{
    // ── GetByMerchantIdAsync ──────────────────────────────────────
    public async Task<IReadOnlyList<MerchantContactDto>> GetByMerchantIdAsync(
        long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Name, Phone, Email, Title, Note, CreatedAt
            FROM MerchantContacts
            WHERE MerchantId = @MerchantId
            ORDER BY CreatedAt ASC
            """;

        var result = await session.Connection.QueryAsync<MerchantContactDto>(
            sql, new { MerchantId = merchantId }, session.Transaction);

        return result.AsList();
    }

    // ── InsertAsync ───────────────────────────────────────────────
    public async Task<long> InsertAsync(
        long merchantId,
        string name,
        string? phone,
        string? email,
        string? title,
        string? note,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO MerchantContacts (MerchantId, Name, Phone, Email, Title, Note)
            VALUES (@MerchantId, @Name, @Phone, @Email, @Title, @Note);
            SELECT SCOPE_IDENTITY();
            """;

        return await session.Connection.ExecuteScalarAsync<long>(
            sql,
            new { MerchantId = merchantId, Name = name, Phone = phone, Email = email, Title = title, Note = note },
            session.Transaction);
    }

    // ── UpdateAsync ───────────────────────────────────────────────
    public async Task UpdateAsync(
        long id,
        string name,
        string? phone,
        string? email,
        string? title,
        string? note,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            UPDATE MerchantContacts
            SET Name = @Name, Phone = @Phone, Email = @Email, Title = @Title, Note = @Note
            WHERE Id = @Id
            """;

        await session.Connection.ExecuteAsync(
            sql,
            new { Id = id, Name = name, Phone = phone, Email = email, Title = title, Note = note },
            session.Transaction);
    }

    // ── DeleteAsync ───────────────────────────────────────────────
    public async Task DeleteAsync(long id, IDbSession session, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM MerchantContacts WHERE Id = @Id";

        await session.Connection.ExecuteAsync(
            sql, new { Id = id }, session.Transaction);
    }

    // ── BelongsToMerchantAsync ────────────────────────────────────
    public async Task<bool> BelongsToMerchantAsync(
        long id, long merchantId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(1) FROM MerchantContacts
            WHERE Id = @Id AND MerchantId = @MerchantId
            """;

        var count = await session.Connection.ExecuteScalarAsync<int>(
            sql, new { Id = id, MerchantId = merchantId }, session.Transaction);

        return count > 0;
    }
}
