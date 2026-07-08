using Application.Abstractions.Persistence;
using Application.Merchants.DTOs;
using Common.Pagination;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IMerchantRepository
{
    Task<Merchant?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得使用者所屬的 Merchant（用於登入後取得組織資訊）。</summary>
    Task<Merchant?> GetByUserIdAsync(long userId, IDbSession session, CancellationToken ct = default);

    /// <summary>分頁查詢業者列表，支援關鍵字與審核狀態篩選。</summary>
    Task<(IReadOnlyList<MerchantListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        VerificationStatus? verificationStatus,
        string? industryType,
        DateTime? dateFrom,
        bool? hasCredit,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得全局 KPI 摘要（全部/啟用中/停用中 數量）。</summary>
    Task<MerchantSummaryDto> GetSummaryAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>
    /// 取得業者詳情頁所需的基本資料（Merchants JOIN Users）。
    /// 聯絡窗口、統計、案件、錢包、成員、活動紀錄由各自 Repository 查詢。
    /// </summary>
    Task<MerchantDetailBaseDto?> GetDetailBaseAsync(long merchantId, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Merchant merchant, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Merchant merchant, IDbSession session, CancellationToken ct = default);
}
