namespace Application.FileStorage;

/// <summary>
/// 案件附件實體檔案儲存抽象。
/// 負責把上傳檔案寫入磁碟 / Blob，以及依檔案中繼資料讀回 Stream。
/// </summary>
public interface ICaseFileStorage
{
    /// <summary>儲存上傳檔案，回傳可供寫入 Files 資料表的中繼資訊。</summary>
    Task<CaseStorageFileInfo> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        long uploadedByUserId,
        CancellationToken ct = default);

    /// <summary>依相對路徑讀取檔案內容。</summary>
    Task<CaseStorageFileContent> OpenAsync(
        string relativePath,
        CancellationToken ct = default);

    /// <summary>刪除實體檔案；若檔案不存在視為成功。</summary>
    Task DeleteAsync(
        string relativePath,
        CancellationToken ct = default);
}
