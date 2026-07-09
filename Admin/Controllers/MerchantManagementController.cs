using Admin.ViewModels.Merchant;
using Application.Merchants.Commands;
using Application.Merchants.Queries;
using Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class MerchantManagementController(
    GetMerchantListHandler listHandler,
    GetMerchantSummaryHandler summaryHandler,
    GetMerchantDetailHandler detailHandler,
    SuspendMerchantHandler suspendHandler,
    UnsuspendMerchantHandler unsuspendHandler,
    UpdateMerchantHandler updateHandler,
    AddMerchantContactHandler addContactHandler,
    UpdateMerchantContactHandler updateContactHandler,
    RemoveMerchantContactHandler removeContactHandler,
    AdjustMerchantCreditHandler adjustCreditHandler) : Controller
{
    // ── GET /MerchantManagement ───────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] MerchantListQueryViewModel vm)
    {
        var query = new GetMerchantListQuery(
            vm.Keyword,
            vm.VerificationStatus,
            vm.IndustryType,
            vm.DateFrom,
            vm.HasCredit,
            vm.Page,
            vm.PageSize);

        var listResult = await listHandler.HandleAsync(query);
        var summaryResult = await summaryHandler.HandleAsync();

        var pageVm = new MerchantIndexViewModel
        {
            List = listResult.Value,
            Summary = summaryResult.Value,
            Query = vm,
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(pageVm);

        return View(pageVm);
    }

    // ── GET /MerchantManagement/Detail/{id} ───────────────
    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        var result = await detailHandler.HandleAsync(new GetMerchantDetailQuery(id));

        if (result.IsFailure)
            return NotFound();

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(result.Value);

        return View(result.Value);
    }

    // ── POST /MerchantManagement/Suspend/{id} ─────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(long id)
    {
        var result = await suspendHandler.HandleAsync(new SuspendMerchantCommand(id));

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Detail), new { id });
        }

        TempData["Success"] = "業者已停用。";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── POST /MerchantManagement/Unsuspend/{id} ───────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsuspend(long id)
    {
        var result = await unsuspendHandler.HandleAsync(new UnsuspendMerchantCommand(id));

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Detail), new { id });
        }

        TempData["Success"] = "業者已解除停用。";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // ── GET /MerchantManagement/Update/{id} ──────────────
    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        var result = await detailHandler.HandleAsync(new GetMerchantDetailQuery(id));

        if (result.IsFailure)
            return NotFound();

        var m = result.Value;
        var vm = new MerchantUpdateViewModel
        {
            MerchantId = m.MerchantId,
            CompanyName = m.CompanyName,
            EnglishName = m.EnglishName,
            TaxId = m.TaxId,
            IndustryType = m.IndustryType,
            ContactName = m.ContactName,
            Phone = m.Phone,
            Fax = m.Fax,
            CompanyEmail = m.CompanyEmail,
            Website = m.Website,
            Address = m.Address,
            EstablishedDate = m.EstablishedDate,
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── POST /MerchantManagement/Update ───────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(MerchantUpdateViewModel vm)
    {
        if (!ModelState.IsValid)
            return await ReturnDetailWithError(vm.MerchantId);

        var cmd = new UpdateMerchantCommand(
            vm.MerchantId,
            vm.CompanyName,
            vm.EnglishName,
            vm.TaxId,
            vm.IndustryType,
            vm.ContactName,
            vm.Phone,
            vm.Fax,
            vm.CompanyEmail,
            vm.Website,
            vm.Address,
            vm.EstablishedDate);

        var result = await updateHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        TempData["Success"] = "業者資料已更新。";
        return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
    }

    // ── POST /MerchantManagement/AddContact ───────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddContact(MerchantContactViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "請確認欄位填寫正確。";
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        var cmd = new AddMerchantContactCommand(
            vm.MerchantId,
            vm.Name,
            vm.Phone,
            vm.Email,
            vm.Title,
            vm.Note);

        var result = await addContactHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        TempData["Success"] = "聯絡窗口已新增。";
        return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
    }

    // ── POST /MerchantManagement/UpdateContact ────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateContact(MerchantContactViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "請確認欄位填寫正確。";
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        var cmd = new UpdateMerchantContactCommand(
            vm.ContactId,
            vm.MerchantId,
            vm.Name,
            vm.Phone,
            vm.Email,
            vm.Title,
            vm.Note);

        var result = await updateContactHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        TempData["Success"] = "聯絡窗口已更新。";
        return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
    }

    // ── POST /MerchantManagement/RemoveContact ────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveContact(long contactId, long merchantId)
    {
        var result = await removeContactHandler.HandleAsync(
            new RemoveMerchantContactCommand(contactId, merchantId));

        if (result.IsFailure)
            TempData["Error"] = result.Error.Description;
        else
            TempData["Success"] = "聯絡窗口已刪除。";

        return RedirectToAction(nameof(Detail), new { id = merchantId });
    }

    // ── POST /MerchantManagement/AdjustCredit ────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustCredit(AdjustCreditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "請確認欄位填寫正確。";
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        var cmd = new AdjustMerchantCreditCommand(
            vm.MerchantId,
            vm.OperationType,
            vm.Amount,
            vm.Reason,
            vm.Note,
            vm.ExpiresAt);

        var result = await adjustCreditHandler.HandleAsync(cmd);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
        }

        TempData["Success"] = vm.OperationType == 1 ? "折扣金加值成功。" : "折扣金扣回成功。";
        return RedirectToAction(nameof(Detail), new { id = vm.MerchantId });
    }

    // ── 私有輔助方法 ──────────────────────────────────────
    private async Task<IActionResult> ReturnDetailWithError(long merchantId)
    {
        var detailResult = await detailHandler.HandleAsync(new GetMerchantDetailQuery(merchantId));
        if (detailResult.IsFailure)
            return NotFound();

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(nameof(Detail), detailResult.Value);

        return View(nameof(Detail), detailResult.Value);
    }
}
