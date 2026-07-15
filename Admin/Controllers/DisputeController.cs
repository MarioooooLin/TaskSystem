using Admin.Extensions;
using Admin.ViewModels.Dispute;
using Application.Disputes.Commands;
using Application.Disputes.Queries;
using Common.Errors;
using Common.Pagination;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class DisputeController(
    GetDisputeListHandler listHandler,
    GetDisputeSummaryHandler summaryHandler,
    GetDisputeDetailHandler detailHandler,
    ResolveDisputeHandler resolveHandler) : Controller
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

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var result = await detailHandler.HandleAsync(new GetDisputeDetailQuery(id));
        if (result.IsFailure)
        {
            return result.Error.Type switch
            {
                ErrorType.NotFound => NotFound(),
                ErrorType.Forbidden => Forbid(),
                _ => StatusCode(500)
            };
        }

        var vm = new DisputeDetailViewModel
        {
            Detail = result.Value,
            Resolve = new ResolveDisputeViewModel { DisputeId = id }
        };

        return PartialView(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(ResolveDisputeViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var detailResult = await detailHandler.HandleAsync(new GetDisputeDetailQuery(vm.DisputeId));
            if (detailResult.IsFailure) return StatusCode(500);

            return PartialView("Detail", new DisputeDetailViewModel
            {
                Detail = detailResult.Value,
                Resolve = vm
            });
        }

        var command = new ResolveDisputeCommand(
            vm.DisputeId,
            vm.Resolution,
            vm.ResolutionNote);

        var result = await resolveHandler.HandleAsync(command);
        if (result.IsFailure)
        {
            var detailResult = await detailHandler.HandleAsync(new GetDisputeDetailQuery(vm.DisputeId));
            if (detailResult.IsFailure) return StatusCode(500);

            ModelState.AddModelError("", result.Error.Description);
            return PartialView("Detail", new DisputeDetailViewModel
            {
                Detail = detailResult.Value,
                Resolve = vm
            });
        }

        // 處理成功後重新載入 drawer 內容
        var refreshed = await detailHandler.HandleAsync(new GetDisputeDetailQuery(vm.DisputeId));
        if (refreshed.IsFailure) return StatusCode(500);

        return PartialView("Detail", new DisputeDetailViewModel
        {
            Detail = refreshed.Value,
            Resolve = new ResolveDisputeViewModel { DisputeId = vm.DisputeId }
        });
    }
}
