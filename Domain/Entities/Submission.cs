using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// 對應 Submissions 資料表。
/// 每次 KOL 提交或重新提交都建立新的一筆。
/// </summary>
public class Submission
{
    public long Id { get; set; }
    public long TaskId { get; set; }
    public long KolId { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public bool IsAutoApproved { get; set; }
    public string? Note { get; set; }
    public string? RejectReason { get; set; }

    public DateTime ReviewDeadlineAt { get; set; }  // SubmittedAt + 14 天
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public long? ReviewedByUserId { get; set; }

    // ── Domain 規則（待實作）────────────────────────────
    public bool CanReview() => Status == SubmissionStatus.Submitted;
    public bool IsOverdue(DateTime now) => Status == SubmissionStatus.Submitted
                                          && now >= ReviewDeadlineAt;
    public bool CanDispute() => Status is SubmissionStatus.Submitted
                                       or SubmissionStatus.RevisionRequested
                                       or SubmissionStatus.Overdue
                                       or SubmissionStatus.Rejected;
}
