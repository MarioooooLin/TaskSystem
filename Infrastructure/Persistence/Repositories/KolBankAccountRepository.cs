using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class KolBankAccountRepository : IKolBankAccountRepository
{
    public async Task<KolBankAccountDto?> GetByKolIdAsync(
        long kolId, IDbSession session, CancellationToken ct = default)
    {
        const string sql = """
            SELECT AccountType, AccountName, BankCode, BankName,
                   -- 遮蔽中間碼：顯示前4碼-****-後4碼
                   LEFT(AccountNumberEncrypted, 4) + '-****-' +
                       RIGHT(AccountNumberEncrypted, 4) AS MaskedAccountNumber,
                   Status, UpdatedAt
            FROM KolBankAccounts
            WHERE KolId = @KolId
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<KolBankAccountDto>(
            sql, new { KolId = kolId }, session.Transaction);
    }
}
