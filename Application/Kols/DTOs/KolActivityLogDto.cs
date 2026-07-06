namespace Application.Kols.DTOs;

/// <summary>KOL 相關活動紀錄列項（詳情頁操作紀錄）。</summary>
public sealed class KolActivityLogDto
{
    public long Id { get; init; }
    public string Action { get; init; } = string.Empty;
    public string? ActorName { get; init; }
    public long? RelatedCaseId { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
