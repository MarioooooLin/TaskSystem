namespace Application.Merchants.Options;

public sealed class MerchantImpersonationOptions
{
    public const string SectionName = "Impersonation";

    /// <summary>一次性票證有效秒數，預設 60 秒。</summary>
    public int TicketLifetimeSeconds { get; set; } = 60;

    /// <summary>代理登入有效分鐘數，預設 30 分鐘。</summary>
    public int ImpersonationLifetimeMinutes { get; set; } = 30;

    /// <summary>Admin 站台 Base URL（Merchant 結束代理時導回用）。</summary>
    public string AdminBaseUrl { get; set; } = string.Empty;

    /// <summary>Merchant 站台 Base URL（Admin 發起代理時自動提交用）。</summary>
    public string MerchantBaseUrl { get; set; } = string.Empty;
}
