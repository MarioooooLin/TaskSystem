using Admin.Extensions;
using Admin.ViewModels.Dispute;
using Application.Disputes.Queries;
using Common.Pagination;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class DisputeController(
    GetDisputeListHandler listHandler,
    GetDisputeSummaryHandler summaryHandler) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(DisputeListQueryViewModel query)
    {
        var summaryResult = await summaryHandler.HandleAsync();
        if (summaryResult.IsFailure)
            return StatusCode(500);

        var pageQuery = new PageQuery(query.Page, query.PageSize);
        var listQuery = new GetDisputeListQuery(
            query.Keyword,
            query.Status,
            query.DisputeType,
            pageQuery);

        var listResult = await listHandler.HandleAsync(listQuery);
        if (listResult.IsFailure)
            return StatusCode(500);

        var vm = new DisputeIndexViewModel
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
