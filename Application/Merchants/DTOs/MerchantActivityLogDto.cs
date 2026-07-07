namespace Application.Merchants.DTOs;

/// <summary>業者相關的活動紀錄列項（詳情頁近期活動）。</summary>
public sealed class MerchantActivityLogDto
{
    public long Id { get; init; }
    public string Action { get; init; } = string.Empty;

    /// <summary>異動對象（來自 ActivityLogs.TargetType）。</summary>
    public string? TargetType { get; init; }

    public string? ActorName { get; init; }
    public long? RelatedCaseId { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
