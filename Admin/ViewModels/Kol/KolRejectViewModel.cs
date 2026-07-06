using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Kol;

/// <summary>退回修改 KOL ViewModel（退回原因必填）。</summary>
public sealed class KolRejectViewModel
{
    public long KolId { get; set; }

    [Required(ErrorMessage = "退回原因為必填。")]
    [MaxLength(500, ErrorMessage = "退回原因最多 500 字。")]
    public string RejectionNote { get; set; } = string.Empty;
}
