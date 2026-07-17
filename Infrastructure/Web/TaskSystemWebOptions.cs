namespace Infrastructure.Web;

public sealed class TaskSystemWebOptions
{
    public required string CookieName { get; init; }
    public required TimeSpan CookieExpireTimeSpan { get; init; }
    public int? LoginPermitLimit { get; init; }
    public bool AlwaysUseSecureCookie { get; init; }
}
