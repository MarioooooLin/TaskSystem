using Application.Abstractions.Persistence;
using Application.Kols.DTOs;

namespace Application.Abstractions.Repositories;

public interface IKolStatsRepository
{
    /// <summary>取得 KOL 統計數字：任務數、完成數、待審核數、爭議數。</summary>
    Task<KolStatsDto> GetStatsByKolIdAsync(
        long kolId,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得 KOL 收益概況（來自 KolWallets）。</summary>
    Task<KolEarningsSummaryDto> GetEarningsSummaryAsync(
        long kolId,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得最新 N 筆任務紀錄（JOIN Cases + Merchants）。</summary>
    Task<IReadOnlyList<KolTaskSummaryDto>> GetRecentTasksAsync(
        long kolId,
        int take,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得最新 N 筆活動紀錄（來自 ActivityLogs，篩選 KOL 相關）。</summary>
    Task<IReadOnlyList<KolActivityLogDto>> GetRecentActivityLogsAsync(
        long kolId,
        int take,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得 KOL 類型清單（來自 KolCategories）。</summary>
    Task<IReadOnlyList<short>> GetCategoriesAsync(
        long kolId,
        IDbSession session,
        CancellationToken ct = default);
}
