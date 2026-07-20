using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// 案件預算快照 Repository（對應 CaseBudgetSnapshots 資料表）。
/// </summary>
public interface ICaseBudgetSnapshotRepository
{
    Task<CaseBudgetSnapshot?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件最新一筆快照。</summary>
    Task<CaseBudgetSnapshot?> GetLatestByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default);

    /// <summary>依 IdempotencyKey 查詢快照。</summary>
    Task<CaseBudgetSnapshot?> GetByIdempotencyKeyAsync(string idempotencyKey, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(CaseBudgetSnapshot snapshot, IDbSession session, CancellationToken ct = default);
}
