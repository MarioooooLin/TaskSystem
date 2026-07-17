namespace Application.Merchants.Queries;

/// <summary>
/// 業者端首頁儀表板查詢。
/// 由登入使用者的 MerchantId 自動取得，不需要傳入參數。
/// </summary>
public sealed record GetMerchantDashboardQuery(long MerchantId);
