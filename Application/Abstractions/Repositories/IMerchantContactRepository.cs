using Application.Abstractions.Persistence;
using Application.Merchants.DTOs;

namespace Application.Abstractions.Repositories;

public interface IMerchantContactRepository
{
    Task<IReadOnlyList<MerchantContactDto>> GetByMerchantIdAsync(
        long merchantId,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>新增聯絡窗口，回傳新 Id。</summary>
    Task<long> InsertAsync(
        long merchantId,
        string name,
        string? phone,
        string? email,
        string? title,
        string? note,
        IDbSession session,
        CancellationToken ct = default);

    Task UpdateAsync(
        long id,
        string name,
        string? phone,
        string? email,
        string? title,
        string? note,
        IDbSession session,
        CancellationToken ct = default);

    Task DeleteAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>確認聯絡窗口是否屬於指定 Merchant（防止越權刪除）。</summary>
    Task<bool> BelongsToMerchantAsync(long id, long merchantId, IDbSession session, CancellationToken ct = default);
}
