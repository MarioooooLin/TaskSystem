using Application.Abstractions.Persistence;
using Application.Dashboard.DTOs;

namespace Application.Abstractions.Repositories;

/// <summary>營運總覽唯讀查詢 Repository。</summary>
public interface IDashboardRepository
{
    /// <summary>取得營運總覽頁完整資料。</summary>
    Task<DashboardDto> GetDashboardAsync(int topK, IDbSession session, CancellationToken ct = default);
}
