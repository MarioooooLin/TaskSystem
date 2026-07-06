using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// 對應 CaseApplications 資料表。
/// </summary>
public class Application
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public long KolId { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    public string? Message { get; set; }
    public bool IsRequirementMatched { get; set; } = true;
    public string? MismatchReasons { get; set; }  // JSON
    public DateTime? ReconfirmedAt { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public long? ReviewedByUserId { get; set; }

    // ── Domain 規則（待實作）────────────────────────────
    public bool CanAccept() => Status == ApplicationStatus.Applied;
    public bool CanReject() => Status is ApplicationStatus.Applied
                                          or ApplicationStatus.PendingReconfirmation;
    public bool CanCancel() => Status is ApplicationStatus.Applied
                                          or ApplicationStatus.Accepted
                                          or ApplicationStatus.PendingReconfirmation;
    public bool CanReconfirm() => Status == ApplicationStatus.PendingReconfirmation;
}
