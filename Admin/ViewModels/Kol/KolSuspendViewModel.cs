using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Kol;

/// <summary>停權 KOL ViewModel（停權原因必填）。</summary>
public sealed class KolSuspendViewModel
{
    public long KolId { get; set; }

    [Required(ErrorMessage = "停權原因為必填。")]
    [MaxLength(500, ErrorMessage = "停權原因最多 500 字。")]
    public string SuspensionNote { get; set; } = string.Empty;
}
