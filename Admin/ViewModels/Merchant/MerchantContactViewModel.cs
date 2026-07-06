using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.Merchant;

/// <summary>新增 / 編輯業者聯絡窗口 ViewModel。</summary>
public sealed class MerchantContactViewModel
{
    /// <summary>編輯時填入；新增時為 0。</summary>
    public long ContactId { get; set; }

    public long MerchantId { get; set; }

    [Required(ErrorMessage = "姓名為必填。")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [EmailAddress(ErrorMessage = "Email 格式不正確。")]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
