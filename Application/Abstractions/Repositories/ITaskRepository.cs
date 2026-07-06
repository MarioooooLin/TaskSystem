using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// Task Repository 介面。
/// 對應 Tasks 資料表；Entity 命名 CaseTask 避免與 System.Threading.Tasks.Task 衝突。
/// </summary>
public interface ITaskRepository
{
    Task<CaseTask?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件下第一筆狀態為 PendingMatch 且尚未綁定的 Task。</summary>
    Task<CaseTask?> GetFirstPendingMatchAsync(long caseId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件下所有 Task。</summary>
    Task<IReadOnlyList<CaseTask>> GetByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default);

    Task InsertManyAsync(IEnumerable<CaseTask> tasks, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(CaseTask task, IDbSession session, CancellationToken ct = default);

    /// <summary>批次更新（例如案件取消時，所有 Task 改為 Cancelled）。</summary>
    Task UpdateManyAsync(IEnumerable<CaseTask> tasks, IDbSession session, CancellationToken ct = default);
}
