using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<Domain.Entities.User?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    Task<Domain.Entities.User?> GetByEmailAsync(string email, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Domain.Entities.User user, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.User user, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 批次停用指定業者底下所有 Active MerchantMembers 對應的 Users.Status = Suspended。
    /// 業者停用時呼叫，在同一 transaction 內執行。
    /// </summary>
    Task SuspendUsersByMerchantAsync(long merchantId, IDbSession session, CancellationToken ct = default);
}
