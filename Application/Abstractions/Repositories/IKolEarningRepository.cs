using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IKolEarningRepository
{
    Task<long> InsertAsync(KolEarning earning, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(KolEarning earning, IDbSession session, CancellationToken ct = default);

    /// <summary>取得指定 Task 的收益紀錄。</summary>
    Task<KolEarning?> GetByTaskIdAsync(long taskId, IDbSession session, CancellationToken ct = default);
}
