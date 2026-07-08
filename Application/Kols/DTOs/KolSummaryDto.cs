namespace Application.Kols.DTOs;

/// <summary>KOL 管理 KPI 摘要（全域統計，不受篩選影響，ADM-005）。</summary>
public sealed class KolSummaryDto
{
    public int TotalCount { get; init; }
    public int ActiveCount { get; init; }    // VerificationStatus = Approved (2)
    public int PendingCount { get; init; }   // VerificationStatus = Pending (1)
    public int RejectedCount { get; init; }  // VerificationStatus = Rejected (3)
    public int SuspendedCount { get; init; } // VerificationStatus = Suspended (4)
    /// <summary>有未結案爭議（Status IN 1,2）的 KOL 數量。</summary>
    public int AbnormalCount { get; init; }
}
