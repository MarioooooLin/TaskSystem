using Application.Abstractions.Persistence;
using Application.Kols.DTOs;

namespace Application.Abstractions.Repositories;

public interface IKolBankAccountRepository
{
    /// <summary>取得 KOL 收款資料（每位 KOL 只有一筆）。帳號中間碼遮蔽由 Repository 處理。</summary>
    Task<KolBankAccountDto?> GetByKolIdAsync(
        long kolId,
        IDbSession session,
        CancellationToken ct = default);
}
