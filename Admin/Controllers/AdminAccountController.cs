using Admin.ViewModels.AdminAccount;
using Application.Abstractions.Security;
using Application.AdminAccounts.Commands;
using Application.AdminAccounts.DTOs;
using Application.AdminAccounts.Queries;
using Common.Pagination;
using Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Admin.Controllers;

[Authorize]
public sealed class AdminAccountController(
    ICurrentUser currentUser,
    GetAdminAccountListHandler listHandler,
    GetAdminAccountSummaryHandler summaryHandler,
    GetAdminAccountEditHandler editHandler,
    GetAdminRoleOptionsHandler roleOptionsHandler,
    GetRecentAdminAccountLogsHandler logsHandler,
    CreateAdminAccountInvitationHandler createHandler,
    UpdateAdminAccountHandler updateHandler,
    SuspendAdminAccountHandler suspendHandler,
    ActivateAdminAccountHandler activateHandler,
    ResendAdminAccountInvitationHandler resendHandler) : Controller
{
    private const string ViewPermission = "Admin.Account.View";
    private const string ManagePermission = "Admin.Account.Manage";
    private const string ChangeStatusPermission = "Admin.Account.ChangeStatus";

    // ── GET /AdminAccount/Index ───────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(
        string? keyword = null,
        Domain.Enums.UserStatus? status = null,
        string? department = null,
        long? roleId = null,
        int page = 1)
    {
        if (!currentUser.HasPermission(ViewPermission))
            return Forbid();

        var query = new AdminAccountListQueryViewModel
        {
            Keyword = keyword,
            Status = status,
            Department = department,
            RoleId = roleId,
            Page = page
        };

        var listResult = await listHandler.HandleAsync(new GetAdminAccountListQuery(
            query.Keyword,
            query.Status,
            query.Department,
            query.RoleId,
            query.Page,
            query.PageSize));

        var summaryResult = await summaryHandler.HandleAsync();
        var rolesResult = await roleOptionsHandler.HandleAsync();
        var logsResult = await logsHandler.HandleAsync(count: 10);

        if (listResult.IsFailure || summaryResult.IsFailure || rolesResult.IsFailure || logsResult.IsFailure)
            return StatusCode(500);

        var vm = new AdminAccountIndexViewModel
        {
            List = listResult.Value,
            Summary = summaryResult.Value,
            Query = query,
            AvailableRoles = rolesResult.Value,
            RecentLogs = logsResult.Value,
            SuccessMessage = TempData["SuccessMessage"] as string,
            ErrorMessage = TempData["ErrorMessage"] as string
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── GET /AdminAccount/Create ──────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var rolesResult = await roleOptionsHandler.HandleAsync();
        if (rolesResult.IsFailure)
            return StatusCode(500);

        var vm = new CreateAdminAccountViewModel
        {
            AvailableRoles = MapRoleOptions(rolesResult.Value)
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── POST /AdminAccount/Create ─────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAdminAccountViewModel vm)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var rolesResult = await roleOptionsHandler.HandleAsync();
        if (rolesResult.IsFailure)
            return StatusCode(500);

        vm.AvailableRoles = MapRoleOptions(rolesResult.Value);

        if (!ModelState.IsValid)
        {
            if (Request.Headers.ContainsKey("HX-Request"))
                return PartialView(vm);
            return View(vm);
        }

        var cmd = new CreateAdminAccountInvitationCommand(
            vm.Name,
            vm.Email,
            vm.RoleId,
            vm.Department,
            vm.JobTitle,
            vm.Phone,
            vm.Note);

        var result = await createHandler.HandleAsync(cmd);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error.Description);
            if (Request.Headers.ContainsKey("HX-Request"))
                return PartialView(vm);
            return View(vm);
        }

        TempData["SuccessMessage"] = "已建立帳號並發送邀請。";

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var listResult = await listHandler.HandleAsync(new GetAdminAccountListQuery(null, null, null, null, 1, 20));
            var summaryResult = await summaryHandler.HandleAsync();
            if (listResult.IsFailure || summaryResult.IsFailure)
                return StatusCode(500);

            var indexVm = new AdminAccountIndexViewModel
            {
                List = listResult.Value,
                Summary = summaryResult.Value,
                Query = new AdminAccountListQueryViewModel(),
                AvailableRoles = rolesResult.Value,
                RecentLogs = [],
                SuccessMessage = TempData["SuccessMessage"] as string
            };
            return PartialView("Index", indexVm);
        }

        return RedirectToAction(nameof(Index));
    }

    // ── GET /AdminAccount/Edit/{id} ───────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var editResult = await editHandler.HandleAsync(new GetAdminAccountEditQuery(id));
        var rolesResult = await roleOptionsHandler.HandleAsync();

        if (editResult.IsFailure)
        {
            if (editResult.Error.Type == Common.Errors.ErrorType.NotFound)
                return NotFound();
            return StatusCode(500);
        }

        if (rolesResult.IsFailure)
            return StatusCode(500);

        var dto = editResult.Value;
        var vm = new EditAdminAccountViewModel
        {
            UserId = dto.UserId,
            Name = dto.Name,
            Email = dto.Email,
            RoleIds = dto.RoleIds.ToArray(),
            Department = dto.Department,
            JobTitle = dto.JobTitle,
            Phone = dto.Phone,
            Note = dto.Note,
            AvailableRoles = MapRoleOptions(rolesResult.Value)
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── POST /AdminAccount/Edit/{id} ──────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EditAdminAccountViewModel vm)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var rolesResult = await roleOptionsHandler.HandleAsync();
        if (rolesResult.IsFailure)
            return StatusCode(500);

        vm.AvailableRoles = MapRoleOptions(rolesResult.Value);

        if (!ModelState.IsValid)
        {
            if (Request.Headers.ContainsKey("HX-Request"))
                return PartialView(vm);
            return View(vm);
        }

        var cmd = new UpdateAdminAccountCommand(
            id,
            vm.Name,
            vm.Email,
            vm.RoleIds,
            vm.Department,
            vm.JobTitle,
            vm.Phone,
            vm.Note);

        var result = await updateHandler.HandleAsync(cmd);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error.Description);
            if (Request.Headers.ContainsKey("HX-Request"))
                return PartialView(vm);
            return View(vm);
        }

        TempData["SuccessMessage"] = "帳號資料已更新。";

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var listResult = await listHandler.HandleAsync(new GetAdminAccountListQuery(null, null, null, null, 1, 20));
            var summaryResult = await summaryHandler.HandleAsync();
            if (listResult.IsFailure || summaryResult.IsFailure)
                return StatusCode(500);

            var indexVm = new AdminAccountIndexViewModel
            {
                List = listResult.Value,
                Summary = summaryResult.Value,
                Query = new AdminAccountListQueryViewModel(),
                AvailableRoles = rolesResult.Value,
                RecentLogs = [],
                SuccessMessage = TempData["SuccessMessage"] as string
            };
            return PartialView("Index", indexVm);
        }

        return RedirectToAction(nameof(Index));
    }

    // ── POST /AdminAccount/Suspend/{id} ───────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(long id)
    {
        if (!currentUser.HasPermission(ChangeStatusPermission))
            return Forbid();

        var result = await suspendHandler.HandleAsync(new SuspendAdminAccountCommand(id));
        SetOperationMessage(result, "帳號已停用。", "停用帳號失敗：");
        return RedirectToAction(nameof(Index));
    }

    // ── POST /AdminAccount/Activate/{id} ──────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(long id)
    {
        if (!currentUser.HasPermission(ChangeStatusPermission))
            return Forbid();

        var result = await activateHandler.HandleAsync(new ActivateAdminAccountCommand(id));
        SetOperationMessage(result, "帳號已啟用。", "啟用帳號失敗：");
        return RedirectToAction(nameof(Index));
    }

    // ── POST /AdminAccount/Resend/{id} ────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend(long id)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var result = await resendHandler.HandleAsync(new ResendAdminAccountInvitationCommand(id));
        SetOperationMessage(result, "邀請已重新發送。", "重新發送邀請失敗：");
        return RedirectToAction(nameof(Index));
    }

    private static List<SelectListItem> MapRoleOptions(IReadOnlyList<AdminRoleOptionDto> roles)
    {
        return roles.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = r.Name
        }).ToList();
    }

    private void SetOperationMessage(Result result, string successText, string failurePrefix)
    {
        if (result.IsSuccess)
            TempData["SuccessMessage"] = successText;
        else
            TempData["ErrorMessage"] = failurePrefix + result.Error.Description;
    }
}
