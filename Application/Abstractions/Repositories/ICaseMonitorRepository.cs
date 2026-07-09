using Application.Abstractions.Persistence;
using Application.Cases.DTOs;
using Common.Pagination;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

/// <summary>
/// 案件監控唯讀查詢 Repository（Admin 後台列表、KPI、警示）。
/// 與 ICaseRepository（Entity 寫入）分離，不參與 Transaction 寫入。
/// </summary>
public interface ICaseMonitorRepository
{
    /// <summary>取得案件列表（分頁 + 篩選）。</summary>
    Task<(IReadOnlyList<CaseListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        CaseStatus? status,
        bool? hasPendingReview,
        bool? hasCommission,
        DateTime? dateFrom,
        DateTime? dateTo,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得 KPI 統計（各狀態案件數）。</summary>
    Task<CaseSummaryDto> GetSummaryAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>取得警示卡數字（待驗收、逾期、爭議）。</summary>
    Task<CaseAlertDto> GetAlertAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件詳情（含任務清單、附件、操作紀錄）。找不到時回傳 null。</summary>
    Task<CaseDetailDto?> GetDetailAsync(long caseId, IDbSession session, CancellationToken ct = default);
}
