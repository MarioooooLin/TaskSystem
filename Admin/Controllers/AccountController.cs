using Admin.ViewModels.Account;
using Application.Account;
using Common.Primitives;
using Domain.Exceptions;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Admin.Controllers;

public sealed class AccountController(
    LoginHandler loginHandler,
    TaskSystemSignInService signInService,
    ValidateInvitationTokenHandler validateTokenHandler,
    SetPasswordHandler setPasswordHandler,
    ProfileHandler profileHandler,
    ChangePasswordHandler changePasswordHandler) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

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

        var cmd = new LoginCommand(vm.Email.Trim(), vm.Password);
        var result = await loginHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
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

        await signInService.SignInAsync(
            HttpContext,
            loginResult.UserId,
            loginResult.Name,
            Domain.Enums.AccountType.Admin,
            loginResult.PermissionCodes,
            vm.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
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

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView();

        return View();
    }

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
