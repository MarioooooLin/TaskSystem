using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// 檔案中繼資料 Repository（對應 Files 資料表）。
/// 實體檔案存取由 ICaseFileStorage 負責。
/// </summary>
public interface IFileRepository
{
    Task<FileEntity?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(FileEntity file, IDbSession session, CancellationToken ct = default);

    Task DeleteAsync(long id, IDbSession session, CancellationToken ct = default);
}
