using Application.Abstractions.Persistence;
using Application.Merchants.DTOs;

namespace Application.Abstractions.Repositories;

public interface IMerchantStatsRepository
{
    /// <summary>查詢業者統計：案件數、任務數、爭議數、完成率（Admin 詳情頁用）。</summary>
    Task<MerchantStatsDto> GetStatsByMerchantIdAsync(
        long merchantId,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得最新 N 筆案件摘要。</summary>
    Task<IReadOnlyList<MerchantCaseSummaryDto>> GetRecentCasesAsync(
        long merchantId,
        int take,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得最新 N 筆活動紀錄（來自 ActivityLogs，篩選 MerchantId 相關案件）。</summary>
    Task<IReadOnlyList<MerchantActivityLogDto>> GetRecentActivityLogsAsync(
        long merchantId,
        int take,
        IDbSession session,
        CancellationToken ct = default);
}
