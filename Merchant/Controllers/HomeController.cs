using Application.Abstractions.Security;
using Application.Merchants.Queries;
using Infrastructure.Authentication;
using Merchant.Models;
using Merchant.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Merchant.Controllers;

[Authorize]
public sealed class HomeController(
    GetMerchantDashboardHandler dashboardHandler) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var merchantIdClaim = User.FindFirstValue(TaskSystemClaimTypes.MerchantId);
        if (!long.TryParse(merchantIdClaim, out var merchantId))
            return Unauthorized();

        var result = await dashboardHandler.HandleAsync(new GetMerchantDashboardQuery(merchantId));

        if (result.IsFailure)
            return StatusCode(500);

        var data = result.Value;

        var vm = new IndexViewModel
        {
            CompanyName = data.CompanyName,
            StatusLabel = data.StatusLabel,
            Wallet = new IndexViewModel.WalletViewModel
            {
                AvailableAmount = data.Wallet.AvailableAmount,
                FrozenAmount = data.Wallet.FrozenAmount,
                TotalAmount = data.Wallet.TotalAmount
            },
            StatusCounts = data.StatusCounts.Select(s => new IndexViewModel.StatusCountViewModel
            {
                Category = (int)s.Category,
                Label = s.Label,
                IconUrl = s.IconUrl,
                Count = s.Count
            }).ToList(),
            Todos = data.Todos.Select(t => new IndexViewModel.TodoViewModel
            {
                CaseId = t.CaseId,
                TodoType = t.TodoType,
                Title = t.Title,
                Status = t.Status,
                StatusCssClass = t.StatusCssClass,
                CreatedAt = t.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                ActionText = t.ActionText,
                ActionUrl = t.ActionUrl
            }).ToList(),
            RecentCases = data.RecentCases.Select(c => new IndexViewModel.RecentCaseViewModel
            {
                CaseId = c.CaseId,
                TypeLabel = c.TypeLabel,
                Title = c.Title,
                Status = c.Status,
                StatusCssClass = c.StatusCssClass,
                CreatedAt = c.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                ActionText = c.ActionText,
                ActionUrl = c.ActionUrl
            }).ToList(),
            PendingReviewCount = data.PendingReviewCount
        };

        ViewData["CompanyName"] = vm.CompanyName;
        ViewData["AvailableAmount"] = vm.Wallet.AvailableAmount;

        return View(vm);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
