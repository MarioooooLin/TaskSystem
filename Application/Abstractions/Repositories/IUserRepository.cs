using Application.Abstractions.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<Domain.Entities.User?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    Task<Domain.Entities.User?> GetByEmailAsync(string email, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Domain.Entities.User user, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.User user, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 依 Token 與 Email 查詢一筆待處理的 Admin 帳號邀請。
    /// </summary>
    Task<UserInvitation?> GetPendingInvitationByTokenAsync(
        string token, string email, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 將邀請標記為已接受。
    /// </summary>
    Task AcceptInvitationAsync(
        long invitationId, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 更新使用者密碼雜湊。
    /// </summary>
    Task UpdatePasswordAsync(
        long userId, string passwordHash, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 取得指定使用者的角色名稱清單（限 Scope = System 的後台角色）。
    /// </summary>
    Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(
        long userId, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 批次停用指定業者底下所有 Active MerchantMembers 對應的 Users.Status = Suspended。
    /// 業者停用時呼叫，在同一 transaction 內執行。
    /// </summary>
    Task SuspendUsersByMerchantAsync(long merchantId, IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// Restore suspended users under active merchant members when the merchant is unsuspended.
    /// </summary>
    Task ReactivateUsersByMerchantAsync(long merchantId, IDbSession session, CancellationToken ct = default);
}
