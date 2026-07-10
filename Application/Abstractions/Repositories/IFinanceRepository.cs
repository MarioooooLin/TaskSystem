using Application.Abstractions.Persistence;
using Application.Finance.DTOs;
using Common.Pagination;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

/// <summary>
/// 帳務總覽唯讀查詢 Repository（Admin 後台財務摘要與帳務列表）。
/// </summary>
public interface IFinanceRepository
{
    /// <summary>取得平台財務摘要。</summary>
    Task<FinanceSummaryDto> GetSummaryAsync(
        DateTime dateFrom,
        DateTime dateTo,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得帳務列表（分頁 + 篩選）。</summary>
    Task<(IReadOnlyList<FinanceListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        CaseStatus? status,
        DateTime dateFrom,
        DateTime dateTo,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default);
}
