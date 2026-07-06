using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得指定 Task 最新一筆 Submission（KOL 重提時會新增一筆）。</summary>
    Task<Submission?> GetLatestByTaskIdAsync(long taskId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得所有到期且仍為 Submitted 的 Submission（自動驗收排程用）。</summary>
    Task<IReadOnlyList<Submission>> GetOverdueAsync(DateTime utcNow, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Submission submission, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Submission submission, IDbSession session, CancellationToken ct = default);
}
