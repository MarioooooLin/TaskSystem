using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Dispute;

public sealed class DisputeListQueryViewModel
{
    [Display(Name = "關鍵字")]
    public string? Keyword { get; set; }

    [Display(Name = "異議狀態")]
    public DisputeStatus? Status { get; set; }

    [Display(Name = "異議類型")]
    public string? DisputeType { get; set; }

    [Display(Name = "頁碼")]
    public int Page { get; set; } = 1;

    [Display(Name = "每頁筆數")]
    public int PageSize { get; set; } = 20;
}
