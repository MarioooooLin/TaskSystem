using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMerchantImpersonationTicketRepository
{
    /// <summary>建立票證，回傳新 Id。</summary>
    Task<long> InsertAsync(
        MerchantImpersonationTicket ticket,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>以 TokenHash 查詢票證（不檢查是否過期或已使用）。</summary>
    Task<MerchantImpersonationTicket?> GetByTokenHashAsync(
        string tokenHash,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>
    /// 原子兌換票證。僅當票證存在、未過期且未使用時更新 UsedAtUtc。
    /// 回傳 true 表示兌換成功。
    /// </summary>
    Task<bool> TryRedeemAsync(
        long ticketId,
        DateTime usedAtUtc,
        IDbSession session,
        CancellationToken ct = default);
}
