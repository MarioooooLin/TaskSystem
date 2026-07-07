using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class KolSocialAccountRepository : IKolSocialAccountRepository
{
    public async Task<IReadOnlyList<KolSocialAccountDto>> GetByKolIdAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, Platform, AccountName, FollowersCount,
                   DataSource, VerificationStatus, LastSyncAt, UpdatedAt
            FROM KolSocialAccounts
            WHERE KolId = @KolId
            ORDER BY Platform ASC
            """;

        var result = await session.Connection.QueryAsync<KolSocialAccountDto>(
            sql, new { KolId = kolId }, session.Transaction);

        return result.AsList();
    }
}
