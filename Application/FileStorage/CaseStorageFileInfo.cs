namespace Application.FileStorage;

/// <summary>上傳檔案後回傳的中繼資訊。</summary>
public sealed class CaseStorageFileInfo
{
    public long FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public int FileSize { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
}

/// <summary>從儲存空間讀取的檔案內容。</summary>
public sealed class CaseStorageFileContent
{
    public Stream Stream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public long Length { get; init; }
}
