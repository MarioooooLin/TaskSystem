using Admin.ViewModels.CaseMonitor;
using Application.Cases.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class CaseMonitorController(
    GetCaseListHandler listHandler,
    GetCaseSummaryHandler summaryHandler) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CaseListQueryViewModel query)
    {
        var summaryResult = await summaryHandler.HandleAsync();
        if (summaryResult.IsFailure)
            return StatusCode(500);

        var listQuery = new GetCaseListQuery(
            query.Keyword,
            query.Status,
            query.HasPendingReview,
            query.HasCommission,
            query.DateFrom,
            query.DateTo,
            query.Page,
            query.PageSize);

        var listResult = await listHandler.HandleAsync(listQuery);
        if (listResult.IsFailure)
            return StatusCode(500);

        var vm = new CaseMonitorIndexViewModel
        {
            Summary = summaryResult.Value.Summary,
            Alert = summaryResult.Value.Alert,
            List = listResult.Value,
            Query = query
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Detail(long id)
    {
        return View();
    }
}
