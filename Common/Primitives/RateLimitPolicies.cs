namespace Common.Primitives;

/// <summary>
/// Rate Limit Policy 名稱常數。
/// 三個 MVC 專案共用，避免字串散落。
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>登入端點：60 秒內最多 5 次（預設值，可由設定覆寫）。</summary>
    public const string Login = "rl_login";

    /// <summary>忘記密碼端點：5 分鐘內最多 3 次。</summary>
    public const string ForgotPassword = "rl_forgot_password";

    /// <summary>全域 API 防護：10 秒內最多 100 次。</summary>
    public const string Global = "rl_global";
}
