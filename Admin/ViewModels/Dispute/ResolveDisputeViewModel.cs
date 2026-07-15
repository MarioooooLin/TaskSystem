using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Dispute;

public sealed class ResolveDisputeViewModel
{
    public long DisputeId { get; set; }

    [Required(ErrorMessage = "請選擇處理結果")]
    [Display(Name = "處理結果")]
    public DisputeStatus Resolution { get; set; }

    [Required(ErrorMessage = "平台處理意見為必填")]
    [Display(Name = "平台處理意見")]
    public string ResolutionNote { get; set; } = string.Empty;
}
