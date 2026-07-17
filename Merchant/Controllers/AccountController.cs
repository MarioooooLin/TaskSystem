using Application.Account;
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
using System.Security.Claims;

namespace Merchant.Controllers;

public sealed class AccountController(MerchantLoginHandler loginHandler, TaskSystemSignInService signInService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

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
}
