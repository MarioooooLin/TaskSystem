using System.Security.Claims;

namespace Infrastructure.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static bool IsImpersonating(this ClaimsPrincipal user)
        => user.FindFirstValue(TaskSystemClaimTypes.IsImpersonating) == "true";

    public static bool IsImpersonationReadOnly(this ClaimsPrincipal user)
        => user.FindFirstValue(TaskSystemClaimTypes.ImpersonationReadOnly) == "true";

    public static long? GetOriginalAdminUserId(this ClaimsPrincipal user)
        => long.TryParse(user.FindFirstValue(TaskSystemClaimTypes.OriginalAdminUserId), out var id)
            ? id
            : null;

    public static string? GetOriginalAdminName(this ClaimsPrincipal user)
        => user.FindFirstValue(TaskSystemClaimTypes.OriginalAdminName);

    public static DateTime? GetImpersonationExpiresAtUtc(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(TaskSystemClaimTypes.ImpersonationExpiresAt);
        if (!long.TryParse(value, out var seconds))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }
}
