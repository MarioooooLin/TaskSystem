using Application.Abstractions.Persistence;
using Application.Kols.DTOs;
using Common.Pagination;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IKolRepository
{
    Task<KolProfile?> GetByIdAsync(long kolId, IDbSession session, CancellationToken ct = default);

    /// <summary>分頁查詢 KOL 列表（ADM-005），支援關鍵字、狀態、類型（多選）、平台（多選）、日期篩選。</summary>
    Task<(IReadOnlyList<KolListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        VerificationStatus? verificationStatus,
        IReadOnlyList<short>? categories,
        IReadOnlyList<short>? platforms,
        bool? hasBankAccount,
        DateTime? dateFrom,
        DateTime? dateTo,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>分頁查詢待審核 KOL（ADM-015），支援關鍵字、狀態 chip、類型、平台、送審日期篩選。</summary>
    Task<(IReadOnlyList<KolReviewListItemDto> Items, int TotalCount)> GetReviewListAsync(
        string? keyword,
        string? statusFilter,
        short? category,
        short? platform,
        DateOnly? submittedDate,
        PageQuery page,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>審核新進 KOL 頁面 KPI 摘要（ADM-015）。</summary>
    Task<KolReviewSummaryDto> GetReviewSummaryAsync(IDbSession session, CancellationToken ct = default);

    /// <summary>取得 KOL 詳情頁基本資料（KolProfiles JOIN Users，LEFT JOIN 審核 Admin）。</summary>
    Task<KolDetailBaseDto?> GetDetailBaseAsync(long kolId, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(KolProfile kol, IDbSession session, CancellationToken ct = default);

    /// <summary>全域 KPI 摘要（不受篩選影響，ADM-005）。</summary>
    Task<KolSummaryDto> GetSummaryAsync(IDbSession session, CancellationToken ct = default);
}
