using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// 對應 CaseAttachments 資料表，關聯案件與上傳檔案。
/// </summary>
public sealed class CaseAttachment
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public long FileId { get; set; }
    public CaseAttachmentType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>關聯的檔案中繼資料（查詢時填入）。</summary>
    public FileEntity? File { get; set; }
}
