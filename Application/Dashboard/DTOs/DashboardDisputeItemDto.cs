using Domain.Enums;

namespace Application.Dashboard.DTOs;

/// <summary>營運總覽頁異議待辦列項。</summary>
public sealed class DashboardDisputeItemDto
{
    public long DisputeId { get; init; }
    public string DisputeNo { get; init; } = string.Empty;
    public long CaseId { get; init; }
    public string CaseTitle { get; init; } = string.Empty;
    public string MerchantName { get; init; } = string.Empty;
    public string DisputeType { get; init; } = string.Empty;
    public DisputeStatus Status { get; init; }
    public DateTime OpenedAt { get; init; }
}
