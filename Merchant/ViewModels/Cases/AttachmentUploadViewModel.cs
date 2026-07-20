using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Merchant.ViewModels.Cases;

/// <summary>案件附件上傳 ViewModel。</summary>
public sealed class AttachmentUploadViewModel
{
    public long CaseId { get; set; }

    public IFormFile? File { get; set; }

    public CaseAttachmentType AttachmentType { get; set; } = CaseAttachmentType.ReferenceMaterial;
}
