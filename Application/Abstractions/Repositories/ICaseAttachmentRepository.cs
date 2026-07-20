using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// 案件附件關聯 Repository（對應 CaseAttachments 資料表）。
/// </summary>
public interface ICaseAttachmentRepository
{
    Task<CaseAttachment?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    Task<IReadOnlyList<CaseAttachment>> GetByCaseIdAsync(long caseId, IDbSession session, CancellationToken ct = default);

    Task<int> CountByCaseAndTypeAsync(long caseId, short type, IDbSession session, CancellationToken ct = default);

    Task<int> CountTotalSizeByCaseAsync(long caseId, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(CaseAttachment attachment, IDbSession session, CancellationToken ct = default);

    Task DeleteAsync(long id, IDbSession session, CancellationToken ct = default);
}
