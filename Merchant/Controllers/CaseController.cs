using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Cases.Budget;
using Application.Cases.Commands;
using Application.Cases.DTOs;
using Application.Cases.Queries;
using Application.FileStorage;
using Application.SystemSettings.Queries;
using Common.Primitives;
using Infrastructure.Authentication;
using Merchant.ViewModels.Cases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Merchant.Controllers;

[Authorize]
[EnableRateLimiting(RateLimitPolicies.Global)]
public sealed class CaseController(
    ICurrentUser currentUser,
    GetMerchantCaseListHandler listHandler,
    GetMerchantCaseSummaryHandler summaryHandler,
    GetCaseEditHandler editHandler,
    GetPublishPreviewHandler previewHandler,
    SaveCaseDraftHandler saveDraftHandler,
    PublishCaseHandler publishHandler,
    UploadCaseAttachmentHandler uploadAttachmentHandler,
    DeleteCaseAttachmentHandler deleteAttachmentHandler,
    GetSystemSettingsHandler settingsHandler,
    GetActiveLanguagesHandler languagesHandler,
    ICaseFileStorage caseFileStorage,
    ICaseAttachmentRepository caseAttachmentRepo,
    IUnitOfWork unitOfWork,
    IMerchantWalletRepository walletRepo,
    IMerchantCreditWalletRepository creditWalletRepo) : Controller
{
    private const string MerchantCaseManagePermission = "Merchant.Case.Manage";
    private const string MerchantCasePublishPermission = "Merchant.Case.Publish";

    [HttpGet]
    public async Task<IActionResult> Index(CaseListQueryViewModel query)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        var summaryResult = await summaryHandler.HandleAsync(new GetMerchantCaseSummaryQuery(merchantId));
        if (summaryResult.IsFailure)
            return StatusCode(500);

        var listQuery = new GetMerchantCaseListQuery(
            merchantId,
            query.Keyword,
            query.Status,
            query.ClosedOnly,
            query.RewardTypeFilter,
            query.Platform,
            query.DateFrom,
            query.DateTo,
            query.Page,
            query.PageSize);

        var listResult = await listHandler.HandleAsync(listQuery);
        if (listResult.IsFailure)
            return StatusCode(500);

        var vm = new CaseIndexViewModel
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
    public async Task<IActionResult> Edit(long? id)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        if (!currentUser.HasPermission(MerchantCaseManagePermission))
        {
            TempData["Error"] = "您沒有新增/編輯案件的權限，請聯繫管理員。";
            return RedirectToAction("AccessDenied", "Account", new { returnUrl = Request.Path + Request.QueryString });
        }

        var languageOptions = await LoadLanguageOptionsAsync();

        if (id is null or <= 0)
        {
            return View(new CaseEditViewModel { LanguageOptions = languageOptions });
        }

        var result = await editHandler.HandleAsync(new GetCaseEditQuery(id.Value, merchantId));
        if (result.IsFailure)
            return MapErrorToAction(result.Error);

        var vm = MapToEditViewModel(result.Value);
        vm.LanguageOptions = languageOptions;
        return View(vm);
    }

    private async Task<List<LanguageOptionViewModel>> LoadLanguageOptionsAsync()
    {
        var result = await languagesHandler.HandleAsync(new GetActiveLanguagesQuery());
        if (result.IsFailure)
            return [];

        return result.Value
            .Select(l => new LanguageOptionViewModel { Code = l.Code, DisplayName = l.DisplayName })
            .ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CaseEditViewModel vm)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        if (!currentUser.HasPermission(MerchantCaseManagePermission))
        {
            TempData["Error"] = "您沒有新增/編輯案件的權限，請聯繫管理員。";
            return RedirectToAction("AccessDenied", "Account", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (!ModelState.IsValid)
        {
            vm.LanguageOptions = await LoadLanguageOptionsAsync();
            return View(vm);
        }

        var cmd = new SaveCaseDraftCommand(
            vm.CaseId,
            merchantId,
            currentUser.UserId,
            vm.Title,
            vm.Description,
            vm.CityId,
            vm.Address,
            vm.OfficialUrl,
            vm.Categories.AsReadOnly(),
            vm.Languages.AsReadOnly(),
            vm.Platforms.AsReadOnly(),
            vm.CashRewardAmount.HasValue,
            vm.CashRewardAmount,
            vm.HasCommission,
            vm.CommissionRate,
            vm.CookieDays,
            vm.ApplicationDeadline,
            vm.SubmissionDeadline,
            vm.WantedKolCount,
            vm.DeliverableDescription,
            vm.MinFollowers,
            vm.RequirementNotes,
            vm.BarterItems.Select(b => new CaseBarterItemInput
            {
                Id = b.Id,
                Name = b.Name,
                Quantity = b.Quantity,
                Note = b.Note
            }).ToList().AsReadOnly());

        var result = await saveDraftHandler.HandleAsync(cmd);
        if (result.IsFailure)
        {
            vm.LanguageOptions = await LoadLanguageOptionsAsync();
            ModelState.AddModelError("", result.Error.Description);
            return View(vm);
        }

        if (vm.SubmitMode == "Publish")
            return RedirectToAction(nameof(PublishPreview), new { id = result.Value });

        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> PublishPreview(long id)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        var result = await previewHandler.HandleAsync(new GetPublishPreviewQuery(id, merchantId));
        if (result.IsFailure)
            return MapErrorToAction(result.Error);

        var vm = MapToPublishViewModel(result.Value);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(PublishPreviewViewModel vm)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        if (!currentUser.HasPermission(MerchantCasePublishPermission))
        {
            TempData["Error"] = "您沒有發布案件的權限，請聯繫管理員。";
            return RedirectToAction("AccessDenied", "Account", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (!ModelState.IsValid)
            return View(nameof(PublishPreview), vm);

        var cmd = new PublishCaseCommand(vm.CaseId, merchantId, currentUser.UserId, vm.IdempotencyKey);
        var result = await publishHandler.HandleAsync(cmd);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Error.Description);
            return View(nameof(PublishPreview), vm);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> BudgetEstimate(CaseEditViewModel vm, CancellationToken ct)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        var settingsResult = await settingsHandler.HandleAsync(new GetSystemSettingsQuery(), ct);
        if (settingsResult.IsFailure)
        {
            return PartialView("_BudgetEstimate", new CaseBudgetEstimateViewModel());
        }

        var reward = vm.CashRewardAmount.HasValue ? vm.CashRewardAmount.Value : 0m;
        var count = Math.Max(vm.WantedKolCount, 1);

        await using var uow = await unitOfWork.BeginAsync(ct);
        var creditWallet = await creditWalletRepo.GetByMerchantIdAsync(merchantId, uow.Session, ct);
        var availableCredit = creditWallet?.AvailableAmount ?? 0m;

        var calculator = new CaseBudgetCalculator(settingsResult.Value);
        var breakdown = calculator.Calculate(reward, count, availableCredit);

        var wallet = await walletRepo.GetByMerchantIdAsync(merchantId, uow.Session, ct);
        var balance = wallet?.AvailableAmount ?? 0m;
        await uow.CommitAsync(ct);

        var estimateVm = new CaseBudgetEstimateViewModel
        {
            RewardAmountPerKol = breakdown.RewardAmountPerKol,
            WantedKolCount = breakdown.WantedKolCount,
            RewardSubtotal = breakdown.RewardSubtotal,
            CaseOpeningFee = breakdown.CaseOpeningFee,
            DiscountAmount = breakdown.DiscountAmount,
            PlatformServiceFee = breakdown.PlatformServiceFee,
            EstimatedFrozenAmount = breakdown.EstimatedFrozenAmount,
            CurrentWalletBalance = balance
        };

        return PartialView("_BudgetEstimate", estimateVm);
    }

    [HttpGet]
    public async Task<IActionResult> AttachmentList(long id)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        if (id <= 0)
        {
            return PartialView("_AttachmentList", new List<CaseAttachmentEditViewModel>());
        }

        var result = await editHandler.HandleAsync(new GetCaseEditQuery(id, merchantId));
        if (result.IsFailure)
            return MapErrorToAction(result.Error);

        var attachments = result.Value.Attachments.Select(a => new CaseAttachmentEditViewModel
        {
            Id = a.Id,
            CaseId = id,
            FileId = a.FileId,
            FileName = a.FileName,
            MimeType = a.MimeType,
            FileSize = a.FileSize,
            AttachmentType = a.AttachmentType,
            UploadedAt = a.UploadedAt
        }).ToList();

        return PartialView("_AttachmentList", attachments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(AttachmentUploadViewModel vm)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        if (!currentUser.HasPermission(MerchantCaseManagePermission))
            return Forbid();

        if (vm.File is null || vm.File.Length == 0)
        {
            if (Request.Headers.ContainsKey("HX-Request"))
                return Content("<p class='ac-hint ac-hint--warn'>請選擇檔案</p>");

            return BadRequest("請選擇檔案");
        }

        await using var stream = vm.File.OpenReadStream();
        var cmd = new UploadCaseAttachmentCommand(
            vm.CaseId,
            merchantId,
            currentUser.UserId,
            stream,
            vm.File.FileName,
            vm.File.ContentType,
            vm.File.Length,
            vm.AttachmentType);

        var result = await uploadAttachmentHandler.HandleAsync(cmd);
        if (result.IsFailure)
        {
            if (Request.Headers.ContainsKey("HX-Request"))
                return Content($"<p class='ac-hint ac-hint--warn'>{result.Error.Description}</p>");

            return BadRequest(result.Error.Description);
        }

        if (Request.Headers.ContainsKey("HX-Request"))
            return RedirectToAction(nameof(AttachmentList), new { id = vm.CaseId });

        return RedirectToAction(nameof(Edit), new { id = vm.CaseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(long id, long attachmentId)
    {
        if (!TryGetMerchantId(out var merchantId))
            return Unauthorized();

        if (!currentUser.HasPermission(MerchantCaseManagePermission))
            return Forbid();

        var cmd = new DeleteCaseAttachmentCommand(id, merchantId, currentUser.UserId, attachmentId);
        var result = await deleteAttachmentHandler.HandleAsync(cmd);
        if (result.IsFailure)
            return MapErrorToAction(result.Error);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(long attachmentId)
    {
        await using var uow = await unitOfWork.BeginAsync(HttpContext.RequestAborted);

        var attachment = await caseAttachmentRepo.GetByIdAsync(attachmentId, uow.Session, HttpContext.RequestAborted);
        if (attachment?.File is null)
            return NotFound();

        var content = await caseFileStorage.OpenAsync(attachment.File.FilePath, HttpContext.RequestAborted);
        if (content.Stream == Stream.Null)
            return NotFound();

        return File(content.Stream, attachment.File.MimeType, attachment.File.FileName);
    }

    private bool TryGetMerchantId(out long merchantId)
    {
        var claim = User.FindFirstValue(TaskSystemClaimTypes.MerchantId);
        return long.TryParse(claim, out merchantId);
    }

    private IActionResult MapErrorToAction(Common.Errors.Error error)
    {
        return error.Type switch
        {
            Common.Errors.ErrorType.NotFound => NotFound(),
            Common.Errors.ErrorType.Forbidden => Forbid(),
            Common.Errors.ErrorType.Validation => BadRequest(error.Description),
            Common.Errors.ErrorType.Conflict => Conflict(error.Description),
            _ => StatusCode(500, error.Description)
        };
    }

    private static CaseEditViewModel MapToEditViewModel(CaseEditDto dto)
    {
        return new CaseEditViewModel
        {
            CaseId = dto.CaseId,
            Title = dto.Title,
            Description = dto.Description,
            CityId = dto.CityId,
            Address = dto.Address,
            OfficialUrl = dto.OfficialUrl,
            Categories = dto.Categories.ToList(),
            Languages = dto.Languages.ToList(),
            Platforms = dto.Platforms.ToList(),
            HasCash = dto.HasCash,
            CashRewardAmount = dto.CashRewardAmount,
            HasCommission = dto.HasCommission,
            CommissionRate = dto.CommissionRate,
            CookieDays = dto.CookieDays,
            ApplicationDeadline = dto.ApplicationDeadline,
            SubmissionDeadline = dto.SubmissionDeadline,
            WantedKolCount = dto.WantedKolCount,
            DeliverableDescription = dto.DeliverableDescription,
            MinFollowers = dto.MinFollowers,
            RequirementNotes = dto.RequirementNotes,
            BarterItems = dto.BarterItems.Select(b => new CaseBarterItemEditViewModel
            {
                Id = b.Id,
                Name = b.Name,
                Quantity = b.Quantity,
                Note = b.Note
            }).ToList(),
            Attachments = dto.Attachments.Select(a => new CaseAttachmentEditViewModel
            {
                Id = a.Id,
                FileId = a.FileId,
                FileName = a.FileName,
                MimeType = a.MimeType,
                FileSize = a.FileSize,
                AttachmentType = a.AttachmentType,
                UploadedAt = a.UploadedAt
            }).ToList(),
            Status = dto.Status
        };
    }

    private static PublishPreviewViewModel MapToPublishViewModel(PublishPreviewDto dto)
    {
        return new PublishPreviewViewModel
        {
            CaseId = dto.CaseId,
            Title = dto.Title,
            City = dto.City,
            Address = dto.Address,
            WantedKolCount = dto.WantedKolCount,
            RewardAmountPerKol = dto.RewardAmountPerKol,
            RewardSubtotal = dto.RewardSubtotal,
            HasCommission = dto.HasCommission,
            CommissionRate = dto.CommissionRate,
            BarterItems = dto.BarterItems.Select(b => new PublishPreviewBarterItemViewModel
            {
                Name = b.Name,
                Quantity = b.Quantity,
                Note = b.Note
            }).ToList(),
            Platforms = dto.Platforms.ToList(),
            DeliverableDescription = dto.DeliverableDescription,
            Attachments = dto.Attachments.Select(a => new PublishPreviewAttachmentViewModel
            {
                Id = a.Id,
                FileName = a.FileName,
                FileSize = a.FileSize,
                MimeType = a.MimeType
            }).ToList(),
            FeeItems = dto.FeeItems.Select(f => new PublishPreviewFeeItemViewModel
            {
                Name = f.Name,
                Amount = f.Amount,
                Note = f.Note
            }).ToList(),
            CaseOpeningFee = dto.CaseOpeningFee,
            DiscountAmount = dto.DiscountAmount,
            PlatformServiceFee = dto.PlatformServiceFee,
            EstimatedFrozenAmount = dto.EstimatedFrozenAmount,
            CurrentWalletBalance = dto.CurrentWalletBalance,
            HasEnoughBalance = dto.HasEnoughBalance,
            ApplicationDeadline = dto.ApplicationDeadline,
            SubmissionDeadline = dto.SubmissionDeadline,
            IdempotencyKey = dto.IdempotencyKey
        };
    }
}
