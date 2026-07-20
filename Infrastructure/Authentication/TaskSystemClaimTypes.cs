namespace Infrastructure.Authentication;

public static class TaskSystemClaimTypes
{
    public const string AccountType = "account_type";
    public const string Permission = "permission";
    public const string MerchantId = "merchant_id";
    public const string MerchantName = "merchant_name";

    // ── 管理者代理登入業者端專用 Claims ──────────────────
    public const string IsImpersonating = "is_impersonating";
    public const string ImpersonationReadOnly = "impersonation_read_only";
    public const string OriginalAdminUserId = "original_admin_user_id";
    public const string OriginalAdminName = "original_admin_name";
    public const string ImpersonationExpiresAt = "impersonation_expires_at";
}
