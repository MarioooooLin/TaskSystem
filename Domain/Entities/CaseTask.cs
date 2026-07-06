using Domain.Enums;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Domain.Entities;

/// <summary>
/// 對應 Tasks 資料表。
/// 案件發布時建立 WantedKolCount 筆，初始狀態 PendingMatch。
/// </summary>
public class CaseTask
{
    public long Id { get; set; }
    public long CaseId { get; set; }

    public long? KolId { get; set; }          // null = 尚未綁定
    public long? ApplicationId { get; set; }  // null = 尚未綁定

    public TaskStatus Status { get; set; } = TaskStatus.PendingMatch;

    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // ── Domain 規則（待實作）────────────────────────────
    public bool IsUnbound() => KolId is null && ApplicationId is null;
    public bool CanStartExecution() => Status == TaskStatus.PendingExecution;
}
