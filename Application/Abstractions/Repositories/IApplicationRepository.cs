using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// CaseApplication Repository 介面（對應 CaseApplications 資料表）。
/// C# 類別命名 Application 已用於 Layer，Entity 為 Domain.Entities.Application。
/// </summary>
public interface IApplicationRepository
{
    Task<Domain.Entities.Application?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得指定 KOL 在指定案件的報名紀錄（每案每 KOL 唯一）。</summary>
    Task<Domain.Entities.Application?> GetByCaseAndKolAsync(long caseId, long kolId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件下所有報名紀錄。</summary>
    Task<IReadOnlyList<Domain.Entities.Application>> GetByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件下所有狀態為 Accepted 的報名數量（計算執行門檻用）。</summary>
    Task<int> CountAcceptedAsync(long caseId, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Domain.Entities.Application application, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Domain.Entities.Application application, IDbSession session, CancellationToken ct = default);

    /// <summary>批次更新（案件修改後所有 Accepted 改為 PendingReconfirmation）。</summary>
    Task UpdateManyAsync(IEnumerable<Domain.Entities.Application> applications, IDbSession session, CancellationToken ct = default);
}
