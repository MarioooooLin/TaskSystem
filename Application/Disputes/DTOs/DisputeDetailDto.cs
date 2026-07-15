using Domain.Enums;

namespace Application.Disputes.DTOs;

public sealed class DisputeDetailDto
{
    public long DisputeId { get; set; }
    public string DisputeNo { get; set; } = string.Empty;
    public string DisputeType { get; set; } = string.Empty;
    public DisputeStatus Status { get; set; }

    public long CaseId { get; set; }
    public string CaseNo { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public string CaseSummary { get; set; } = string.Empty;

    public long MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public DisputeContactDto MerchantContact { get; set; } = new();

    public long? KolId { get; set; }
    public string? KolName { get; set; }
    public DisputeContactDto KolContact { get; set; } = new();

    public string MerchantRequirement { get; set; } = string.Empty;
    public string KolSubmission { get; set; } = string.Empty;
    public string MerchantRejectionReason { get; set; } = string.Empty;
    public string KolDisputeReason { get; set; } = string.Empty;
    public IReadOnlyList<DisputeSubmissionItemDto> KolSubmissionItems { get; set; } = Array.Empty<DisputeSubmissionItemDto>();

    public IReadOnlyList<DisputeTimelineDto> Timeline { get; set; } = Array.Empty<DisputeTimelineDto>();
}
