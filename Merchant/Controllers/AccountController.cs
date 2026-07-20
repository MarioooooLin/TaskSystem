using Application.Account;
using Application.Merchants.Commands;
using Application.Merchants.Options;
using Common.Primitives;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Authentication;
using Merchant.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Merchant.Controllers;

public sealed class AccountController(
    MerchantLoginHandler loginHandler,
    RedeemMerchantImpersonationTicketHandler redeemHandler,
    TaskSystemSignInService signInService,
    IOptions<MerchantImpersonationOptions> impersonationOptions) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, bool impersonationExpired = false)
    {
        if (User.Identity?.IsAuthenticated == true && !User.IsImpersonating())
            return RedirectToAction("Index", "Home");

        if (impersonationExpired)
        {
            ModelState.AddModelError(string.Empty, "代理登入已逾時，若需繼續查看請返回 Admin 重新進入。");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        var cmd = new MerchantLoginCommand(vm.TaxId.Trim(), vm.Email.Trim(), vm.Password);
        var result = await loginHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
            var message = result.Error.Code switch
            {
                var c when c == Errors.User.AccountSuspended.Code => Errors.User.AccountSuspended.Description,
                var c when c == Errors.Merchant.NotApproved.Code => Errors.Merchant.NotApproved.Description,
                var c when c == Errors.Member.NotActive.Code => Errors.Member.NotActive.Description,
                _ => Errors.User.InvalidCredentials.Description
            };
            ModelState.AddModelError(string.Empty, message);
            return View(vm);
        }

        var loginResult = result.Value;

        await signInService.SignInAsync(
            HttpContext,
            loginResult.UserId,
            loginResult.Name,
            AccountType.Merchant,
            loginResult.PermissionCodes,
            vm.RememberMe,
            loginResult.Email,
            [
                new Claim(TaskSystemClaimTypes.MerchantId, loginResult.MerchantId.ToString()),
                new Claim(TaskSystemClaimTypes.MerchantName, loginResult.CompanyName)
            ]);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ── POST /Account/RedeemImpersonation ─────────────────
    /// <summary>
    /// 跨站兌換 Admin 發出的一次性代理登入票證。此 Action 為首次跨站 POST，
    /// 不依賴一般 Anti-forgery Token，而以不可猜測、短效且一次性的 token 驗證。
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RedeemImpersonation(string token)
    {
        Response.Headers.CacheControl = "no-store";

        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = Errors.Impersonation.InvalidToken.Description;
            return RedirectToAction(nameof(Login), new { impersonationExpired = true });
        }

        var result = await redeemHandler.HandleAsync(new RedeemMerchantImpersonationTicketCommand(token.Trim()));

        if (result.IsFailure)
        {
            TempData["Error"] = Errors.Impersonation.InvalidToken.Description;
            return RedirectToAction(nameof(Login), new { impersonationExpired = true });
        }

        var data = result.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(impersonationOptions.Value.ImpersonationLifetimeMinutes);

        await signInService.SignInImpersonationAsync(
            HttpContext,
            data.MerchantId,
            data.MerchantName,
            data.AdminUserId,
            data.AdminName,
            expiresAtUtc);

        return RedirectToAction("Index", "Home");
    }

    // ── POST /Account/EndImpersonation ────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndImpersonation()
    {
        if (!User.IsImpersonating())
            return RedirectToAction(nameof(Login));

        var adminUserId = User.GetOriginalAdminUserId();
        var merchantIdClaim = User.FindFirstValue(TaskSystemClaimTypes.MerchantId);

        // 只清除 Merchant Cookie，保留 Admin Cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 導回 Admin 對應業者詳情頁；驗證 URL 為設定檔中的受信任前綴
        var adminBaseUrl = impersonationOptions.Value.AdminBaseUrl.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(adminBaseUrl)
            || !Uri.TryCreate(adminBaseUrl, UriKind.Absolute, out var adminUri))
        {
            return RedirectToAction(nameof(Login));
        }

        var redirectPath = "/MerchantManagement";
        if (long.TryParse(merchantIdClaim, out var merchantId))
            redirectPath = $"/MerchantManagement/Detail/{merchantId}";

        var redirectUrl = adminBaseUrl + redirectPath;
        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var redirectUri)
            || redirectUri.GetLeftPart(UriPartial.Authority) != adminUri.GetLeftPart(UriPartial.Authority))
        {
            return RedirectToAction(nameof(Login));
        }

        return Redirect(redirectUrl);
    }
}
