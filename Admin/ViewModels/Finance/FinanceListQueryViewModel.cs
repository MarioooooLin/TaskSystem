using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Finance;

/// <summary>帳務總覽列表篩選條件 ViewModel。</summary>
public sealed class FinanceListQueryViewModel
{
    [Display(Name = "關鍵字")]
    public string? Keyword { get; set; }

    [Display(Name = "案件狀態")]
    public CaseStatus? Status { get; set; }

    [Display(Name = "開始日期")]
    [DataType(DataType.Date)]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "結束日期")]
    [DataType(DataType.Date)]
    public DateTime? DateTo { get; set; }

    [Display(Name = "頁碼")]
    public int Page { get; set; } = 1;

    [Display(Name = "每頁筆數")]
    public int PageSize { get; set; } = 20;
}
