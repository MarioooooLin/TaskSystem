using Admin.ViewModels.RolePermission;
using Application.Abstractions.Security;
using Application.Roles.Commands;
using Application.Roles.DTOs;
using Application.Roles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Admin.Controllers;

[Authorize]
public sealed class RolePermissionController(
    ICurrentUser currentUser,
    GetAdminRoleListHandler listHandler,
    GetAdminRolePermissionEditHandler editHandler,
    CreateAdminRoleHandler createHandler,
    UpdateAdminRoleHandler updateHandler) : Controller
{
    private const string ViewPermission = "Admin.Role.View";
    private const string ManagePermission = "Admin.Role.Manage";

    // ── GET /RolePermission/Index ─────────────────────────
    [HttpGet]
    public async Task<IActionResult> Index(
        string? keyword = null,
        bool? isActive = null,
        bool? isSystemReserved = null,
        bool? hasHighRiskPermission = null,
        int page = 1)
    {
        if (!currentUser.HasPermission(ViewPermission))
            return Forbid();

        var query = new AdminRoleListQueryViewModel
        {
            Keyword = keyword,
            IsActive = isActive,
            IsSystemReserved = isSystemReserved,
            HasHighRiskPermission = hasHighRiskPermission,
            Page = page
        };

        var result = await listHandler.HandleAsync(new GetAdminRoleListQuery(
            query.Keyword,
            query.IsActive,
            query.IsSystemReserved,
            query.HasHighRiskPermission,
            query.Page,
            query.PageSize));

        if (result.IsFailure)
            return StatusCode(500);

        var vm = new AdminRoleIndexViewModel
        {
            List = result.Value.List,
            Summary = result.Value.Summary,
            Query = query
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── GET /RolePermission/Create ────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var result = await editHandler.HandleAsync(new GetAdminRolePermissionEditQuery(0));
        if (result.IsFailure)
            return StatusCode(500);

        var vm = new CreateAdminRoleViewModel
        {
            PermissionGroups = MapPermissionGroups(result.Value.Permissions)
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── POST /RolePermission/Create ───────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAdminRoleViewModel vm)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        if (!ModelState.IsValid)
        {
            var preload = await editHandler.HandleAsync(new GetAdminRolePermissionEditQuery(0));
            if (preload.IsSuccess)
                vm.PermissionGroups = MapPermissionGroups(preload.Value.Permissions);
            return View(vm);
        }

        var result = await createHandler.HandleAsync(new CreateAdminRoleCommand(
            vm.Name,
            vm.Description,
            vm.IsSystemReserved,
            vm.PermissionIds));

        if (result.IsFailure)
        {
            var preload = await editHandler.HandleAsync(new GetAdminRolePermissionEditQuery(0));
            if (preload.IsSuccess)
                vm.PermissionGroups = MapPermissionGroups(preload.Value.Permissions);
            ModelState.AddModelError("", result.Error.Description);
            return View(vm);
        }

        TempData["SuccessMessage"] = "角色已建立。";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /RolePermission/Edit/{id} ─────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var result = await editHandler.HandleAsync(new GetAdminRolePermissionEditQuery(id));
        if (result.IsFailure)
        {
            if (result.Error.Type == Common.Errors.ErrorType.NotFound)
                return NotFound();
            return StatusCode(500);
        }

        var dto = result.Value;
        var vm = new EditAdminRoleViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IsSystemReserved = dto.IsSystemReserved,
            IsActive = dto.IsActive,
            PermissionIds = dto.SelectedPermissionIds.ToList(),
            PermissionGroups = MapPermissionGroups(dto.Permissions)
        };

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    // ── POST /RolePermission/Edit/{id} ────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, EditAdminRoleViewModel vm)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        if (id != vm.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            var preload = await editHandler.HandleAsync(new GetAdminRolePermissionEditQuery(id));
            if (preload.IsSuccess)
                vm.PermissionGroups = MapPermissionGroups(preload.Value.Permissions);
            return View(vm);
        }

        var result = await updateHandler.HandleAsync(new UpdateAdminRoleCommand(
            vm.Id,
            vm.Name,
            vm.Description,
            vm.IsActive,
            vm.PermissionIds));

        if (result.IsFailure)
        {
            var preload = await editHandler.HandleAsync(new GetAdminRolePermissionEditQuery(id));
            if (preload.IsSuccess)
                vm.PermissionGroups = MapPermissionGroups(preload.Value.Permissions);
            ModelState.AddModelError("", result.Error.Description);
            return View(vm);
        }

        TempData["SuccessMessage"] = "角色已更新。";
        return RedirectToAction(nameof(Index));
    }

    private static List<PermissionGroupViewModel> MapPermissionGroups(IReadOnlyList<AdminPermissionDto> permissions)
    {
        return permissions
            .GroupBy(p => p.GroupName)
            .Select(g => new PermissionGroupViewModel
            {
                GroupName = g.Key,
                Items = g.Select(p => new PermissionItemViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Description = p.Description,
                    RiskLevel = p.RiskLevel
                }).ToList()
            })
            .ToList();
    }
}
