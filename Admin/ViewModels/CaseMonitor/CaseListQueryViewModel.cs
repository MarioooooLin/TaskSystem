using Domain.Enums;

namespace Admin.ViewModels.CaseMonitor;

/// <summary>案件監控列表篩選條件 ViewModel。</summary>
public sealed class CaseListQueryViewModel
{
    public string? Keyword { get; set; }

    /// <summary>
    /// 待驗收任務篩選：null=不限，true=有待驗收，false=無待驗收。
    /// 表單值：""=不限，"true"=有，"false"=無。
    /// </summary>
    public bool? HasPendingReview { get; set; }

    /// <summary>
    /// 導購分潤篩選：null=不限，true=已啟用，false=未啟用。
    /// </summary>
    public bool? HasCommission { get; set; }

    /// <summary>案件狀態篩選（null=全部）。</summary>
    public CaseStatus? Status { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
