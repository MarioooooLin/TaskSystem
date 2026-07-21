using Admin.ViewModels.SystemSetting;
using Application.Abstractions.Security;
using Application.SystemSettings.Commands;
using Application.SystemSettings.DTOs;
using Application.SystemSettings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers;

[Authorize]
public sealed class SystemSettingController(
    GetSystemSettingsHandler settingsHandler,
    GetRecentSystemSettingLogsHandler logsHandler,
    UpdateSystemSettingsHandler updateHandler,
    ResetSystemSettingsHandler resetHandler,
    ICurrentUser currentUser) : Controller
{
    private const string ViewPermission = "Admin.SystemSettings.View";
    private const string ManagePermission = "Admin.SystemSettings.Manage";

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Parameters()
    {
        if (!currentUser.HasPermission(ViewPermission))
            return Forbid();

        var settingsResult = await settingsHandler.HandleAsync(new GetSystemSettingsQuery());
        if (settingsResult.IsFailure)
            return StatusCode(500);

        var logsResult = await logsHandler.HandleAsync(new GetRecentSystemSettingLogsQuery(Count: 10));
        if (logsResult.IsFailure)
            return StatusCode(500);

        var vm = MapToViewModel(settingsResult.Value, logsResult.Value);

        if (Request.Headers.ContainsKey("HX-Request"))
            return PartialView(vm);

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Parameters(SystemSettingParametersViewModel vm)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        if (!ModelState.IsValid)
        {
            vm.RecentLogs = await LoadRecentLogsAsync();
            vm.CanManage = true;
            return View(vm);
        }

        var cmd = new UpdateSystemSettingsCommand(
            vm.CaseOpeningFeeAmount,
            vm.PlatformServiceFeeRate,
            vm.AffiliatePlatformCommissionRate,
            vm.AffiliateKolMinCommissionRate,
            vm.CaseAutoExecutionThresholdRate,
            vm.KolMinPayoutAmount,
            vm.KolTaxRate,
            vm.KolPayoutFeeRate,
            vm.KolPayoutFixedFeeAmount,
            vm.KolPayoutMode,
            vm.KolPayoutDays,
            vm.KolPayoutClosingDayOffset,
            vm.Note);

        var result = await updateHandler.HandleAsync(cmd);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error.Description);
            vm.RecentLogs = await LoadRecentLogsAsync();
            vm.CanManage = true;
            return View(vm);
        }

        TempData["SuccessMessage"] = result.Value.Count > 0
            ? "系統參數已儲存。"
            : "沒有異動的參數，未更新任何設定。";

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var settingsResult = await settingsHandler.HandleAsync(new GetSystemSettingsQuery());
            var logsResult = await logsHandler.HandleAsync(new GetRecentSystemSettingLogsQuery(Count: 10));

            if (settingsResult.IsFailure || logsResult.IsFailure)
                return StatusCode(500);

            var updatedVm = MapToViewModel(settingsResult.Value, logsResult.Value);
            return PartialView(nameof(Parameters), updatedVm);
        }

        return RedirectToAction(nameof(Parameters));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reset(string? note)
    {
        if (!currentUser.HasPermission(ManagePermission))
            return Forbid();

        var result = await resetHandler.HandleAsync(new ResetSystemSettingsCommand(note));
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Error.Description;
        }
        else
        {
            TempData["SuccessMessage"] = "已還原為系統預設值。";
        }

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var settingsResult = await settingsHandler.HandleAsync(new GetSystemSettingsQuery());
            var logsResult = await logsHandler.HandleAsync(new GetRecentSystemSettingLogsQuery(Count: 10));

            if (settingsResult.IsFailure || logsResult.IsFailure)
                return StatusCode(500);

            var updatedVm = MapToViewModel(settingsResult.Value, logsResult.Value);
            return PartialView(nameof(Parameters), updatedVm);
        }

        return RedirectToAction(nameof(Parameters));
    }

    private async Task<IReadOnlyList<SystemSettingLogViewModel>> LoadRecentLogsAsync()
    {
        var result = await logsHandler.HandleAsync(new GetRecentSystemSettingLogsQuery(Count: 10));
        return result.IsSuccess
            ? result.Value.Select(MapLog).ToList()
            : Array.Empty<SystemSettingLogViewModel>();
    }

    private SystemSettingParametersViewModel MapToViewModel(
        SystemSettingValuesDto values,
        IReadOnlyList<SystemSettingLogDto> logs)
    {
        return new SystemSettingParametersViewModel
        {
            CaseOpeningFeeAmount = values.CaseOpeningFeeAmount,
            PlatformServiceFeeRate = values.PlatformServiceFeeRate,
            AffiliatePlatformCommissionRate = values.AffiliatePlatformCommissionRate,
            AffiliateKolMinCommissionRate = values.AffiliateKolMinCommissionRate,
            CaseAutoExecutionThresholdRate = values.CaseAutoExecutionThresholdRate,
            KolMinPayoutAmount = values.KolMinPayoutAmount,
            KolTaxRate = values.KolTaxRate,
            KolPayoutFeeRate = values.KolPayoutFeeRate,
            KolPayoutFixedFeeAmount = values.KolPayoutFixedFeeAmount,
            KolPayoutMode = values.KolPayoutMode,
            KolPayoutDays = values.KolPayoutDays,
            KolPayoutClosingDayOffset = values.KolPayoutClosingDayOffset,
            Note = string.Empty,
            RecentLogs = logs.Select(MapLog).ToList(),
            CanManage = currentUser.HasPermission(ManagePermission),
        };
    }

    private static SystemSettingLogViewModel MapLog(SystemSettingLogDto dto)
    {
        return new SystemSettingLogViewModel
        {
            SettingKey = dto.SettingKey,
            SettingName = dto.SettingName,
            OldValue = dto.OldValue,
            NewValue = dto.NewValue,
            ChangedByName = dto.ChangedByName,
            ChangedAt = dto.ChangedAt,
            Note = dto.Note,
        };
    }
}
