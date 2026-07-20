using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Infrastructure.Authentication;

public sealed class TaskSystemSignInService
{
    private static readonly TimeSpan RememberMeDuration = TimeSpan.FromHours(8);
    private static readonly TimeSpan ImpersonationDuration = TimeSpan.FromMinutes(30);

    public Task SignInAsync(
        HttpContext httpContext,
        long userId,
        string name,
        AccountType accountType,
        IEnumerable<string> permissionCodes,
        bool rememberMe,
        string? email = null,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new(TaskSystemClaimTypes.AccountType, accountType.ToString())
        };

        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new Claim(ClaimTypes.Email, email));

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);

        claims.AddRange(permissionCodes.Select(code => new Claim(TaskSystemClaimTypes.Permission, code)));

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var authProps = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.Add(RememberMeDuration) : null
        };

        return httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps);
    }

    /// <summary>
    /// 建立管理者代理登入業者端的唯讀 Cookie。
    /// 非持久性 Session Cookie、絕對 30 分鐘到期、禁止 Sliding Expiration。
    /// </summary>
    public Task SignInImpersonationAsync(
        HttpContext httpContext,
        long merchantId,
        string merchantName,
        long adminUserId,
        string adminName,
        DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, adminUserId.ToString()),
            new(ClaimTypes.Name, $"{adminName}（代理：{merchantName}）"),
            new(TaskSystemClaimTypes.AccountType, AccountType.Merchant.ToString()),
            new(TaskSystemClaimTypes.MerchantId, merchantId.ToString()),
            new(TaskSystemClaimTypes.MerchantName, merchantName),
            new(TaskSystemClaimTypes.IsImpersonating, "true"),
            new(TaskSystemClaimTypes.ImpersonationReadOnly, "true"),
            new(TaskSystemClaimTypes.OriginalAdminUserId, adminUserId.ToString()),
            new(TaskSystemClaimTypes.OriginalAdminName, adminName),
            new(TaskSystemClaimTypes.ImpersonationExpiresAt,
                new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds().ToString())
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        // 絕對代理期限由 ImpersonationExpiresAt claim 控制；Cookie 保留短暫 grace period，
        // 讓 Middleware 有機會在逾時時攔截並導回 Admin，而不是直接被 Authentication 重新導向 Merchant 登入頁。
        var authProps = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = new DateTimeOffset(expiresAtUtc.AddMinutes(5)),
            AllowRefresh = false,
        };

        return httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps);
    }
}

