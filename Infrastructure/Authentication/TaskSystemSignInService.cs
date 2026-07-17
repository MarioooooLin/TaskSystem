using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Infrastructure.Authentication;

public sealed class TaskSystemSignInService
{
    private static readonly TimeSpan RememberMeDuration = TimeSpan.FromHours(8);

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
}

