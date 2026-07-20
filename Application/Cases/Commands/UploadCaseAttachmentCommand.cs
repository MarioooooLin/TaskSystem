using Domain.Enums;

namespace Application.Cases.Commands;

/// <summary>上傳案件附件。</summary>
public sealed record UploadCaseAttachmentCommand(
    long CaseId,
    long MerchantId,
    long CurrentUserId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize,
    CaseAttachmentType AttachmentType);
