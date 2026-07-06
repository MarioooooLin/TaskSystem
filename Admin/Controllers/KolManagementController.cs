using Admin.ViewModels.Kol;
using Application.Kols.Commands;
using Application.Kols.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class KolManagementController(
    GetKolListHandler listHandler,
    GetKolReviewListHandler reviewListHandler,
    GetKolDetailHandler detailHandler,
    ApproveKolHandler approveHandler,
    RejectKolHandler rejectHandler,
    SuspendKolHandler suspendHandler,
    UnsuspendKolHandler unsuspendHandler) : Controller
{
    // ── GET /KolManagement ─────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] KolListQueryViewModel vm)
    {
        var query = new GetKolListQuery(
            vm.Keyword,
            vm.VerificationStatus,
            vm.Category,
            vm.Platform,
            vm.HasBankAccount,
            vm.Page,
            vm.PageSize);

        var result = await listHandler.HandleAsync(query);
        return View(result.Value);
    }

    // ── GET /KolManagement/ReviewList ──────────────────────
    [HttpGet]
    public async Task<IActionResult> ReviewList([FromQuery] KolReviewListQueryViewModel vm)
    {
        var query = new GetKolReviewListQuery(
            vm.Keyword,
            vm.VerificationStatus,
            vm.Category,
            vm.Platform,
            vm.Page,
            vm.PageSize);

        var result = await reviewListHandler.HandleAsync(query);
        return View(result.Value);
    }

    // ── GET /KolManagement/Detail/{id} ─────────────────────
    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var result = await detailHandler.HandleAsync(new GetKolDetailQuery(id));

        if (result.IsFailure)
            return NotFound();

        return View(result.Value);
    }

    // ── POST /KolManagement/Approve ────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long id)
    {
        var result = await approveHandler.HandleAsync(new ApproveKolCommand(id));

        if (result.IsFailure)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "KOL 審核已通過。";

        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── POST /KolManagement/Reject ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(KolRejectViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "退回原因為必填。";
            return RedirectToAction(nameof(Detail), new { id = vm.KolId });
        }

        var result = await rejectHandler.HandleAsync(
            new RejectKolCommand(vm.KolId, vm.RejectionNote));

        if (result.IsFailure)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "已退回修改，KOL 將收到通知。";

        return RedirectToAction(nameof(Detail), new { id = vm.KolId });
    }

    // ── POST /KolManagement/Suspend ────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(KolSuspendViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "停權原因為必填。";
            return RedirectToAction(nameof(Detail), new { id = vm.KolId });
        }

        var result = await suspendHandler.HandleAsync(
            new SuspendKolCommand(vm.KolId, vm.SuspensionNote));

        if (result.IsFailure)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "KOL 已停權。";

        return RedirectToAction(nameof(Detail), new { id = vm.KolId });
    }

    // ── POST /KolManagement/Unsuspend ──────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsuspend(long id)
    {
        var result = await unsuspendHandler.HandleAsync(new UnsuspendKolCommand(id));

        if (result.IsFailure)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "KOL 停權已解除。";

        return RedirectToAction(nameof(Detail), new { id });
    }
}
