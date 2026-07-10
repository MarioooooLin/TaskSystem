using Domain.Enums;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.Disputes.DTOs;

public sealed class DisputeListItemDto
{
    public long DisputeId { get; set; }
    public string DisputeNo { get; set; } = string.Empty;
    public long CaseId { get; set; }
    public string CaseNo { get; set; } = string.Empty;
    public string CaseTitle { get; set; } = string.Empty;
    public long MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public long? KolId { get; set; }
    public string? KolName { get; set; }
    public DisputeStatus Status { get; set; }
    public string DisputeType { get; set; } = string.Empty;
    public TaskStatus TaskStatus { get; set; }
    public DateTime OpenedAt { get; set; }
}
