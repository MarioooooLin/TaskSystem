namespace Application.Cases.Commands;

/// <summary>刪除案件附件。</summary>
public sealed record DeleteCaseAttachmentCommand(
    long CaseId,
    long MerchantId,
    long CurrentUserId,
    long AttachmentId);
