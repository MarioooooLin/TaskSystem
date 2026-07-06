using Application.Abstractions.Persistence;
using Application.Merchants.DTOs;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMerchantMemberRepository
{
    Task<MerchantMember?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得指定 Merchant 下的特定 User 成員。</summary>
    Task<MerchantMember?> GetByMerchantAndUserAsync(long merchantId, long userId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得指定 Merchant 的全部成員（含 UserName、RoleName）。</summary>
    Task<IReadOnlyList<MerchantMemberItemDto>> GetMemberListAsync(
        long merchantId,
        IDbSession session,
        CancellationToken ct = default);

    Task<long> InsertAsync(MerchantMember member, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(MerchantMember member, IDbSession session, CancellationToken ct = default);
}
