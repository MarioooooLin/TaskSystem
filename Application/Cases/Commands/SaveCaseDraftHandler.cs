using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.SystemSettings.Queries;
using Common.Results;
using Domain.Entities;
using Domain.Enums;

namespace Application.Cases.Commands;

public sealed class SaveCaseDraftHandler(
    IUnitOfWork unitOfWork,
    ICaseRepository caseRepo,
    IApplicationRepository applicationRepo,
    GetSystemSettingsHandler settingsHandler)
{
    public async Task<Result<long>> HandleAsync(
        SaveCaseDraftCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var isNew = cmd.CaseId <= 0;
        Case? originalCase = null;
        Case caseEntity;

        if (isNew)
        {
            caseEntity = new Case
            {
                MerchantId = cmd.MerchantId,
                CreatedByUserId = cmd.CurrentUserId,
                Status = CaseStatus.Draft,
                RecruitmentStatus = RecruitmentStatus.NotOpen,
                ApplicationCount = 0,
                ApprovedAssignmentCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            var editData = await caseRepo.GetEditDataAsync(cmd.CaseId, cmd.MerchantId, uow.Session, ct);
            if (editData is null)
            {
                return Result.Failure<long>(Common.Errors.Error.NotFound("Case.NotFound", "案件不存在或無權限。"));
            }

            caseEntity = editData.Case;
            originalCase = CreateSnapshot(caseEntity);

            if (!caseEntity.CanModify())
            {
                return Result.Failure<long>(Common.Errors.Error.Forbidden("Case.CannotModify", "案件已進入執行階段，無法修改。"));
            }
        }

        // 套用欄位
        caseEntity.Title = cmd.Title;
        caseEntity.Description = cmd.Description;
        caseEntity.City = cmd.CityId?.ToString();
        caseEntity.Address = cmd.Address;
        caseEntity.OfficialUrl = cmd.OfficialUrl;
        caseEntity.WantedKolCount = cmd.WantedKolCount;
        caseEntity.ApplicationDeadline = cmd.ApplicationDeadline;
        caseEntity.SubmissionDeadline = cmd.SubmissionDeadline;
        caseEntity.CashRewardAmount = cmd.HasCash && cmd.CashRewardAmount.HasValue
            ? cmd.CashRewardAmount.Value
            : 0m;
        caseEntity.IsCommissionEnabled = cmd.HasCommission;
        caseEntity.CommissionRate = cmd.HasCommission ? cmd.CommissionRate : null;
        caseEntity.CookieDays = cmd.CookieDays;
        caseEntity.DeliverableDescription = cmd.DeliverableDescription;
        caseEntity.TouchUpdatedAt();

        if (isNew)
        {
            var id = await caseRepo.InsertAsync(caseEntity, uow.Session, ct);
            caseEntity.Id = id;
        }
        else
        {
            await caseRepo.UpdateAsync(caseEntity, uow.Session, ct);
        }

        // 同步子表（以先刪後插簡化草稿階段邏輯）
        await caseRepo.SyncSubtablesAsync(
            caseEntity.Id,
            cmd.Categories,
            cmd.Languages,
            cmd.Platforms,
            cmd.BarterItems,
            cmd.MinFollowers,
            cmd.RequirementNotes,
            uow.Session,
            ct);

        // 若為已發布案件修改且影響權益，已錄取 KOL 改為 PendingReconfirmation
        if (!isNew && caseEntity.Status == CaseStatus.Recruiting && originalCase is not null)
        {
            if (caseEntity.HasSignificantChangesComparedTo(originalCase))
            {
                var settings = await settingsHandler.HandleAsync(new GetSystemSettingsQuery(uow.Session), ct);
                if (settings.IsFailure)
                {
                    return Result.Failure<long>(settings.Error);
                }

                var deadlineDays = settings.Value.CaseReconfirmationDeadlineDays;
                var deadlineAt = DateTime.UtcNow.AddDays(deadlineDays);

                var acceptedCount = await applicationRepo.CountAcceptedAsync(caseEntity.Id, uow.Session, ct);
                if (acceptedCount > 0)
                {
                    await applicationRepo.SetPendingReconfirmationAsync(caseEntity.Id, deadlineAt, uow.Session, ct);
                }
            }
        }

        await uow.CommitAsync(ct);
        return Result.Success(caseEntity.Id);
    }

    private static Case CreateSnapshot(Case source)
    {
        return new Case
        {
            Title = source.Title,
            Description = source.Description,
            OfficialUrl = source.OfficialUrl,
            City = source.City,
            Address = source.Address,
            WantedKolCount = source.WantedKolCount,
            ApplicationDeadline = source.ApplicationDeadline,
            SubmissionDeadline = source.SubmissionDeadline,
            CashRewardAmount = source.CashRewardAmount,
            IsCommissionEnabled = source.IsCommissionEnabled,
            CommissionRate = source.CommissionRate,
            CookieDays = source.CookieDays,
            DeliverableDescription = source.DeliverableDescription
        };
    }

}
