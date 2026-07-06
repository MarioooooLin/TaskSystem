using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// Case Repository 介面。
/// 負責取得與儲存需要變更的 Case Entity，參與 Transaction。
/// </summary>
public interface ICaseRepository
{
    Task<Case?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得 Case，確認同時屬於指定 Merchant（Ownership 防護）。</summary>
    Task<Case?> GetByIdAndMerchantAsync(long id, long merchantId, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Case caseEntity, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Case caseEntity, IDbSession session, CancellationToken ct = default);
}
