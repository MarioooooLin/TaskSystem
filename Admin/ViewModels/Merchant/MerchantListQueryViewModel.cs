using Domain.Enums;

namespace Admin.ViewModels.Merchant;

/// <summary>業者列表頁搜尋條件 ViewModel。</summary>
public sealed class MerchantListQueryViewModel
{
    public string? Keyword { get; set; }
    public VerificationStatus? VerificationStatus { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
