namespace Domain.Entities;

/// <summary>
/// 對應 Files 資料表，保存上傳檔案的中繼資料。
/// 實體檔案路徑為相對路徑，實際 Root 由外部設定決定。
/// </summary>
public sealed class FileEntity
{
    public long Id { get; set; }
    public long UploadedByUserId { get; set; }

    /// <summary>原始檔名。</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>相對於上傳根目錄的檔案路徑。</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>檔案大小（位元組）。</summary>
    public int FileSize { get; set; }

    /// <summary>檔案 MIME Type。</summary>
    public string MimeType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
