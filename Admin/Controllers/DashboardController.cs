using Admin.ViewModels.Dashboard;
using Application.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class DashboardController(GetDashboardHandler dashboardHandler) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await dashboardHandler.HandleAsync(new GetDashboardQuery(TopK: 5));

        if (result.IsFailure)
            return StatusCode(500);

        var vm = new DashboardIndexViewModel { Data = result.Value };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }
}
