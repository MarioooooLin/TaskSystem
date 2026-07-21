using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.Budget;
using Application.Cases.DTOs;
using Application.Helpers;
using Application.SystemSettings.DTOs;
using Application.SystemSettings.Queries;
using Common.Results;
using Domain.Enums;

namespace Application.Cases.Queries;

public sealed class GetPublishPreviewHandler(
    IUnitOfWork unitOfWork,
    ICaseRepository caseRepo,
    IMerchantWalletRepository walletRepo,
    IMerchantCreditWalletRepository creditWalletRepo,
    GetSystemSettingsHandler settingsHandler)
{
    public async Task<Result<PublishPreviewDto>> HandleAsync(
        GetPublishPreviewQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var settingsResult = await settingsHandler.HandleAsync(new GetSystemSettingsQuery(uow.Session), ct);
        if (settingsResult.IsFailure)
        {
            return Common.Results.Result.Failure<PublishPreviewDto>(settingsResult.Error);
        }

        var editData = await caseRepo.GetEditDataAsync(
            query.CaseId,
            query.MerchantId,
            uow.Session,
            ct);

        if (editData is null)
        {
            return Common.Results.Result.Failure<PublishPreviewDto>(Common.Errors.Error.NotFound("Case.NotFound", "案件不存在或無權限。"));
        }

        var c = editData.Case;

        if (!c.CanPublish())
        {
            return Common.Results.Result.Failure<PublishPreviewDto>(Common.Errors.Error.Validation("Case.AlreadyPublished", "案件已發布或無法重複發布。"));
        }

        if (string.IsNullOrWhiteSpace(c.Title)
            || !c.ApplicationDeadline.HasValue
            || !c.SubmissionDeadline.HasValue
            || c.WantedKolCount <= 0
            || c.CashRewardAmount < 0)
        {
            return Common.Results.Result.Failure<PublishPreviewDto>(Common.Errors.Error.Validation("Case.Incomplete", "案件基本資料不完整，無法發布。"));
        }

        var settings = settingsResult.Value;

        var creditWallet = await creditWalletRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var availableCredit = creditWallet?.AvailableAmount ?? 0m;

        var calculator = new CaseBudgetCalculator(settings);
        var breakdown = calculator.Calculate(c.CashRewardAmount, c.WantedKolCount, availableCredit);

        var wallet = await walletRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var balance = wallet?.AvailableAmount ?? 0m;

        var feeItems = JsonSerializer.Deserialize<List<PublishPreviewFeeItemDto>>(
            breakdown.FeeItemsJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? [];

        var dto = new PublishPreviewDto
        {
            CaseId = c.Id,
            Title = c.Title,
            City = TaiwanCity.GetName(c.City),
            Address = c.Address,
            WantedKolCount = breakdown.WantedKolCount,
            RewardAmountPerKol = breakdown.RewardAmountPerKol,
            RewardSubtotal = breakdown.RewardSubtotal,
            HasCommission = c.IsCommissionEnabled,
            CommissionRate = c.CommissionRate,
            BarterItems = editData.BarterItems
                .Select(b => new PublishPreviewBarterItemDto
                {
                    Name = b.Name,
                    Quantity = b.Quantity,
                    Note = b.Note
                }).ToList().AsReadOnly(),
            Platforms = editData.Platforms.AsReadOnly(),
            DeliverableDescription = c.DeliverableDescription,
            Attachments = editData.Attachments
                .Select(a => new PublishPreviewAttachmentDto
                {
                    Id = a.Id,
                    FileName = a.File?.FileName ?? string.Empty,
                    FileSize = a.File?.FileSize ?? 0,
                    MimeType = a.File?.MimeType ?? string.Empty
                }).ToList().AsReadOnly(),
            FeeItems = feeItems.AsReadOnly(),
            CaseOpeningFee = breakdown.CaseOpeningFee,
            DiscountAmount = breakdown.DiscountAmount,
            PlatformServiceFee = breakdown.PlatformServiceFee,
            EstimatedFrozenAmount = breakdown.EstimatedFrozenAmount,
            CurrentWalletBalance = balance,
            HasEnoughBalance = balance >= breakdown.EstimatedFrozenAmount,
            ApplicationDeadline = c.ApplicationDeadline.Value,
            SubmissionDeadline = c.SubmissionDeadline.Value,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        };

        await uow.CommitAsync(ct);
        return Common.Results.Result.Success(dto);
    }
}
