using Application.Abstractions.Persistence;
using Application.Merchants.DTOs;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMerchantWalletRepository
{
    /// <summary>取得 Merchant 錢包，通常搭配 WITH (UPDLOCK) 防止並發問題。</summary>
    Task<MerchantWallet?> GetByMerchantIdAsync(long merchantId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得最新 N 筆交易紀錄（含關聯案件標題）。</summary>
    Task<IReadOnlyList<MerchantWalletTransactionDto>> GetRecentTransactionsAsync(
        long merchantId,
        int take,
        IDbSession session,
        CancellationToken ct = default);

    Task UpdateAsync(MerchantWallet wallet, IDbSession session, CancellationToken ct = default);
}
