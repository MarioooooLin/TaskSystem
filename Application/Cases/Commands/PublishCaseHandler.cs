using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.Budget;
using Application.SystemSettings.DTOs;
using Application.SystemSettings.Queries;
using Common.Results;
using Domain.Entities;
using Domain.Enums;

namespace Application.Cases.Commands;

public sealed class PublishCaseHandler(
    IUnitOfWork unitOfWork,
    ICaseRepository caseRepo,
    ICaseBudgetSnapshotRepository snapshotRepo,
    IMerchantWalletRepository walletRepo,
    GetSystemSettingsHandler settingsHandler)
{
    public async Task<Result> HandleAsync(
        PublishCaseCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var editData = await caseRepo.GetEditDataAsync(cmd.CaseId, cmd.MerchantId, uow.Session, ct);
        if (editData is null)
        {
            return Result.Failure(Common.Errors.Error.NotFound("Case.NotFound", "案件不存在或無權限。"));
        }

        var c = editData.Case;

        if (!c.CanPublish())
        {
            return Result.Failure(Common.Errors.Error.Validation("Case.AlreadyPublished", "案件已發布或無法重複發布。"));
        }

        if (string.IsNullOrWhiteSpace(c.Title)
            || !c.ApplicationDeadline.HasValue
            || !c.SubmissionDeadline.HasValue
            || c.WantedKolCount <= 0
            || c.CashRewardAmount < 0)
        {
            return Result.Failure(Common.Errors.Error.Validation("Case.Incomplete", "案件基本資料不完整，無法發布。"));
        }

        if (c.ApplicationDeadline.Value <= DateTime.UtcNow)
        {
            return Result.Failure(Common.Errors.Error.Validation("Case.DeadlineInvalid", "報名截止日必須在未來。"));
        }

        // 冪等性檢查：同一把 Key 已存在快照，視為重複提交並回傳成功
        if (!string.IsNullOrEmpty(cmd.IdempotencyKey))
        {
            var exists = await snapshotRepo.GetByIdempotencyKeyAsync(cmd.IdempotencyKey, uow.Session, ct);
            if (exists is not null)
            {
                await uow.CommitAsync(ct);
                return Result.Success();
            }
        }

        var settingsResult = await settingsHandler.HandleAsync(new GetSystemSettingsQuery(), ct);
        if (settingsResult.IsFailure)
        {
            return Result.Failure(settingsResult.Error);
        }

        var settings = settingsResult.Value;
        var calculator = new CaseBudgetCalculator(settings);
        var breakdown = calculator.Calculate(c.CashRewardAmount, c.WantedKolCount);

        // 鎖定錢包餘額
        var wallet = await walletRepo.GetByMerchantIdAsync(cmd.MerchantId, uow.Session, ct);
        if (wallet is null)
        {
            return Result.Failure(Common.Errors.Error.Problem("Wallet.NotFound", "業者錢包不存在。"));
        }

        if (!wallet.HasSufficientBalance(breakdown.EstimatedFrozenAmount))
        {
            return Result.Failure(Common.Errors.Error.Validation("Wallet.InsufficientBalance", "錢包餘額不足以發布案件。"));
        }

        wallet.AvailableAmount -= breakdown.EstimatedFrozenAmount;
        wallet.FrozenAmount += breakdown.EstimatedFrozenAmount;
        await walletRepo.UpdateAsync(wallet, uow.Session, ct);

        await walletRepo.InsertTransactionAsync(
            cmd.MerchantId,
            (short)MerchantWalletTransactionType.TaskBudgetFreeze,
            breakdown.EstimatedFrozenAmount,
            (short)MerchantWalletTransactionStatus.Completed,
            c.Id,
            $"案件 {c.Title} 發布預算凍結",
            cmd.CurrentUserId,
            uow.Session,
            ct);

        // 寫入預算快照
        var snapshot = new CaseBudgetSnapshot
        {
            CaseId = c.Id,
            RewardAmountPerKol = breakdown.RewardAmountPerKol,
            WantedKolCount = breakdown.WantedKolCount,
            RewardSubtotal = breakdown.RewardSubtotal,
            FeeItems = breakdown.FeeItemsJson,
            EstimatedFrozenAmount = breakdown.EstimatedFrozenAmount,
            SettingsSnapshot = JsonSerializer.Serialize(settings, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            IdempotencyKey = cmd.IdempotencyKey,
            CreatedAt = DateTime.UtcNow
        };
        await snapshotRepo.InsertAsync(snapshot, uow.Session, ct);

        // 更新案件狀態
        c.Status = CaseStatus.Recruiting;
        c.RecruitmentStatus = RecruitmentStatus.Open;
        c.PublishedAt = DateTime.UtcNow;
        c.TouchUpdatedAt();
        await caseRepo.UpdateAsync(c, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
