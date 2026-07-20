using Application.Abstractions.Persistence;
using Application.Merchants.DTOs;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMerchantCreditWalletRepository
{
    Task<MerchantCreditWallet?> GetByMerchantIdAsync(
        long merchantId, IDbSession session, CancellationToken ct = default);

    Task<MerchantCreditWalletSummaryDto> GetSummaryAsync(
        long merchantId, IDbSession session, CancellationToken ct = default);

    Task<IReadOnlyList<MerchantCreditTransactionDto>> GetRecentGrantsAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default);

    Task<IReadOnlyList<MerchantCreditTransactionDto>> GetRecentUsagesAsync(
        long merchantId, int take, IDbSession session, CancellationToken ct = default);

    /// <summary>新增或更新折扣金錢包（UPSERT）。</summary>
    Task UpsertAsync(MerchantCreditWallet wallet, IDbSession session, CancellationToken ct = default);

    /// <summary>更新折扣金錢包。</summary>
    Task UpdateAsync(MerchantCreditWallet wallet, IDbSession session, CancellationToken ct = default);

    /// <summary>寫入折扣金交易流水。</summary>
    Task InsertTransactionAsync(
        long merchantId, short type, decimal amount,
        long? relatedCaseId, string? reason, string? note,
        long? createdByUserId,
        IDbSession session, CancellationToken ct = default);
}
