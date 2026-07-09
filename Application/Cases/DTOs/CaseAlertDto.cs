namespace Application.Cases.DTOs;

/// <summary>案件監控頁警示卡數字。</summary>
public sealed class CaseAlertDto
{
    /// <summary>待驗收任務數（Tasks.Status = 4 UnderReview）。</summary>
    public int PendingReviewTaskCount { get; init; }

    /// <summary>已逾期任務數（Tasks.Status = 3 InProgress 且 Case.SubmissionDeadline 已過）。</summary>
    public int OverdueTaskCount { get; init; }

    /// <summary>爭議中任務數（Disputes.Status IN (1=Open, 2=UnderReview)）。</summary>
    public int DisputeTaskCount { get; init; }

    /// <summary>導購同步異常案件數（本期保留為 0，尚無同步紀錄表）。</summary>
    public int CommissionSyncErrorCount { get; init; }
}
