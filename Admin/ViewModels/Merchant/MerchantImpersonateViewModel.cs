namespace Admin.ViewModels.Merchant;

/// <summary>Admin 發起代理登入後，自動提交至 Merchant 的過渡頁模型。</summary>
public sealed class MerchantImpersonateViewModel
{
    public string PlainToken { get; init; } = string.Empty;

    public string MerchantName { get; init; } = string.Empty;

    /// <summary>Merchant 站台的票證兌換 endpoint 絕對 URL。</summary>
    public string RedeemUrl { get; init; } = string.Empty;
}
