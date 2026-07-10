using Admin.ViewModels.Finance;
using Application.Finance.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class FinanceController(GetFinanceListHandler listHandler) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(FinanceListQueryViewModel query)
    {
        var listQuery = new GetFinanceListQuery(
            query.Keyword,
            query.Status,
            query.DateFrom,
            query.DateTo,
            query.Page,
            query.PageSize);

        var result = await listHandler.HandleAsync(listQuery);
        if (result.IsFailure)
            return StatusCode(500);

        var (summary, list) = result.Value;

        var vm = new FinanceIndexViewModel
        {
            Summary = summary,
            List = list,
            Query = query
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }
}
