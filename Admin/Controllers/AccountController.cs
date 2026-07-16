using Admin.ViewModels.Account;
using Application.Account;
using Common.Primitives;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Admin.Controllers;

public sealed class AccountController(
    LoginHandler loginHandler,
    ValidateInvitationTokenHandler validateTokenHandler,
    SetPasswordHandler setPasswordHandler,
    ProfileHandler profileHandler,
    ChangePasswordHandler changePasswordHandler) : Controller
{
    // ── GET /Account/Login ────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // 已登入 → 直接去 Dashboard
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // ── POST /Account/Login ───────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        var cmd = new LoginCommand(vm.Email.Trim(), vm.Password);
        var result = await loginHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
            // 所有登入失敗統一顯示在表單頂層，不指定欄位（避免洩漏細節）
            var message = result.Error.Code switch
            {
                var c when c == Errors.User.AccountSuspended.Code => Errors.User.AccountSuspended.Description,
                var c when c == Errors.User.NotAdminAccount.Code => Errors.User.NotAdminAccount.Description,
                _ => Errors.User.InvalidCredentials.Description
            };
            ModelState.AddModelError(string.Empty, message);
            return View(vm);
        }

        var loginResult = result.Value;

        // ── 建立 Claims ──────────────────────────────────
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, loginResult.UserId.ToString()),
            new(ClaimTypes.Name,           loginResult.Name),
            new("account_type",            Domain.Enums.AccountType.Admin.ToString())
        };

        // 每個權限碼一個 Claim
        foreach (var code in loginResult.PermissionCodes)
            claims.Add(new Claim("permission", code));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = vm.RememberMe,
            // RememberMe 維持 8 小時；否則依 Cookie 設定的 60 分鐘 Sliding
            ExpiresUtc = vm.RememberMe
                ? DateTimeOffset.UtcNow.AddHours(8)
                : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // ── POST /Account/Logout ──────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    // ── GET /Account/AccessDenied ─────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView();

        return View();
    }

    // ── GET /Account/SetPassword ──────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword(string token, string email)
    {
        var result = await validateTokenHandler.HandleAsync(
            new ValidateInvitationTokenQuery(token, email));

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(new SetPasswordViewModel
            {
                Token = token,
                Email = email
            });
        }

        return View(new SetPasswordViewModel
        {
            Token = token,
            Email = result.Value.Email,
            Name = result.Value.Name
        });
    }

    // ── POST /Account/SetPassword ─────────────────────────
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await setPasswordHandler.HandleAsync(
            new SetPasswordCommand(vm.Token, vm.Email, vm.Password));

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(vm);
        }

        return RedirectToAction(nameof(Login));
    }

    // ── GET /Account/Profile ──────────────────────────────
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await profileHandler.HandleAsync(new ProfileQuery(userId));
        if (result.IsFailure)
            return StatusCode(StatusCodes.Status500InternalServerError);

        var vm = new ProfileViewModel
        {
            Name = result.Value.Name,
            Email = result.Value.Email,
            RolesDisplay = result.Value.RolesDisplay
        };

        if (TempData["SuccessMessage"] is string message)
            ViewData["SuccessMessage"] = message;

        return View(vm);
    }

    // ── POST /Account/Profile ─────────────────────────────
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Profile(ProfileViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await changePasswordHandler.HandleAsync(
            new ChangePasswordCommand(userId, vm.CurrentPassword, vm.NewPassword));

        if (result.IsFailure)
        {
            var message = result.Error.Code switch
            {
                var c when c == Errors.User.InvalidCredentials.Code => "目前密碼不正確",
                _ => result.Error.Description
            };

            ModelState.AddModelError(string.Empty, message);
            return View(vm);
        }

        TempData["SuccessMessage"] = "密碼已更新，請使用新密碼重新登入。";
        return RedirectToAction(nameof(Profile));
    }
}
