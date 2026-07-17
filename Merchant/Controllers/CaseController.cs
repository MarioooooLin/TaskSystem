using Application.Cases.Queries;
using Infrastructure.Authentication;
using Merchant.ViewModels.Cases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Merchant.Controllers;

[Authorize]
public sealed class CaseController(
    GetMerchantCaseListHandler listHandler,
    GetMerchantCaseSummaryHandler summaryHandler) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CaseListQueryViewModel query)
    {
        var merchantIdClaim = User.FindFirstValue(TaskSystemClaimTypes.MerchantId);
        if (!long.TryParse(merchantIdClaim, out var merchantId))
            return Unauthorized();

        var summaryResult = await summaryHandler.HandleAsync(new GetMerchantCaseSummaryQuery(merchantId));
        if (summaryResult.IsFailure)
            return StatusCode(500);

        var listQuery = new GetMerchantCaseListQuery(
            merchantId,
            query.Keyword,
            query.Status,
            query.ClosedOnly,
            query.RewardTypeFilter,
            query.Platform,
            query.DateFrom,
            query.DateTo,
            query.Page,
            query.PageSize);

        var listResult = await listHandler.HandleAsync(listQuery);
        if (listResult.IsFailure)
            return StatusCode(500);

        var vm = new CaseIndexViewModel
        {
            Summary = summaryResult.Value,
            List = listResult.Value,
            Query = query
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }
}
