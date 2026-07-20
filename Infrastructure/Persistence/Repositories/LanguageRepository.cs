using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class LanguageRepository : ILanguageRepository
{
    public async Task<IReadOnlyList<Language>> GetActiveAsync(
        IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Code, DisplayName, IsActive, SortOrder
            FROM Languages
            WHERE IsActive = 1
            ORDER BY SortOrder, DisplayName
            """;

        var result = await session.Connection.QueryAsync<Language>(
            sql, transaction: session.Transaction);

        return result.AsList();
    }
}
