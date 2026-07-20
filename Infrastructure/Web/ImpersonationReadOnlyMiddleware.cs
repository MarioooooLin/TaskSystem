using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.Options;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;

namespace Infrastructure.Web;

/// <summary>
/// Merchant 站台專用 Middleware：當使用者處於管理者代理登入的唯讀模式時，
/// 攔截所有 POST / PUT / PATCH / DELETE 請求（結束代理登入除外），回傳 403。
/// 同時檢查代理登入是否已逾時，逾時時清除 Merchant Cookie 並導回 Admin。
/// </summary>
public sealed class ImpersonationReadOnlyMiddleware(
    RequestDelegate next,
    IOptions<MerchantImpersonationOptions> options)
{
    private static readonly HashSet<string> ReadOnlyHttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (!user.Identity?.IsAuthenticated == true || !user.IsImpersonationReadOnly())
        {
            await next(context);
            return;
        }

        var expiresAt = user.GetImpersonationExpiresAtUtc();
        if (expiresAt.HasValue && DateTime.UtcNow >= expiresAt.Value)
        {
            await HandleExpiredAsync(context);
            return;
        }

        var method = context.Request.Method;
        if (ReadOnlyHttpMethods.Contains(method))
        {
            await next(context);
            return;
        }

        // 允許結束代理登入的 POST 例外
        if (IsEndImpersonationEndpoint(context))
        {
            await next(context);
            return;
        }

        await BlockWriteAttemptAsync(context);
    }

    private static bool IsEndImpersonationEndpoint(HttpContext context)
    {
        var path = context.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
            return false;

        // 比對 /Account/EndImpersonation 或 /{culture}/Account/EndImpersonation 等變體
        var normalized = path.TrimEnd('/').ToLowerInvariant();
        return context.Request.Method == HttpMethods.Post
               && normalized.EndsWith("/account/endimpersonation", StringComparison.Ordinal);
    }

    private async Task HandleExpiredAsync(HttpContext context)
    {
        var adminUserId = context.User.GetOriginalAdminUserId();
        var merchantIdClaim = context.User.FindFirstValue(TaskSystemClaimTypes.MerchantId);

        // 清除 Merchant Cookie，保留 Admin Cookie
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        await WriteActivityLogAsync(
            context,
            adminUserId,
            merchantIdClaim,
            "ImpersonationExpired",
            context.Request.Path);

        var adminBaseUrl = options.Value.AdminBaseUrl.TrimEnd('/');
        var returnUrl = $"{adminBaseUrl}/MerchantManagement?impersonationExpired=true";

        context.Response.StatusCode = StatusCodes.Status302Found;
        context.Response.Headers.Location = returnUrl;
        context.Response.Headers.CacheControl = "no-store";
    }

    private async Task BlockWriteAttemptAsync(HttpContext context)
    {
        var adminUserId = context.User.GetOriginalAdminUserId();
        var merchantIdClaim = context.User.FindFirstValue(TaskSystemClaimTypes.MerchantId);

        await WriteActivityLogAsync(
            context,
            adminUserId,
            merchantIdClaim,
            "ImpersonationWriteBlocked",
            context.Request.Path,
            context.Request.Method);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.WriteAsync("代理登入期間僅供瀏覽，無法執行寫入操作。");
    }

    private static async Task WriteActivityLogAsync(
        HttpContext context,
        long? adminUserId,
        string? merchantIdClaim,
        string action,
        string? path,
        string? httpMethod = null)
    {
        if (!adminUserId.HasValue)
            return;

        var note = $"Path={path}";
        if (!string.IsNullOrEmpty(httpMethod))
            note += $"; Method={httpMethod}";

        long? targetId = null;
        if (long.TryParse(merchantIdClaim, out var merchantId))
            targetId = merchantId;

        try
        {
            await using var scope = context.RequestServices.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var activityLogRepo = scope.ServiceProvider.GetRequiredService<IActivityLogRepository>();

            await using var uow = await unitOfWork.BeginAsync();

            await activityLogRepo.WriteAsync(
                targetType: "MerchantImpersonation",
                targetId: targetId ?? 0,
                actorUserId: adminUserId.Value,
                action: action,
                note: note,
                session: uow.Session);

            await uow.CommitAsync();
        }
        catch
        {
            // ActivityLog 寫入失敗不應影響攔截行為
        }
    }
}
