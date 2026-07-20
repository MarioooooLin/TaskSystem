using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Results;
using Domain.Enums;

namespace Application.Cases.Queries;

public sealed class GetCaseEditHandler(
    IUnitOfWork unitOfWork,
    ICaseRepository caseRepo)
{
    public async Task<Result<CaseEditDto>> HandleAsync(
        GetCaseEditQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var editData = await caseRepo.GetEditDataAsync(
            query.CaseId,
            query.MerchantId,
            uow.Session,
            ct);

        if (editData is null)
        {
            return Common.Results.Result.Failure<CaseEditDto>(Common.Errors.Error.NotFound("Case.NotFound", "案件不存在或無權限。"));
        }

        var c = editData.Case;

        var dto = new CaseEditDto
        {
            CaseId = c.Id,
            Title = c.Title ?? string.Empty,
            Description = c.Description,
            CityId = TryParseCityId(c.City),
            Address = c.Address,
            OfficialUrl = c.OfficialUrl,
            Categories = editData.Categories.AsReadOnly(),
            Languages = editData.Languages.AsReadOnly(),
            Platforms = editData.Platforms.AsReadOnly(),
            HasCash = c.CashRewardAmount > 0,
            CashRewardAmount = c.CashRewardAmount > 0 ? c.CashRewardAmount : null,
            HasCommission = c.IsCommissionEnabled,
            CommissionRate = c.CommissionRate,
            CookieDays = c.CookieDays,
            ApplicationDeadline = c.ApplicationDeadline,
            SubmissionDeadline = c.SubmissionDeadline,
            WantedKolCount = c.WantedKolCount,
            DeliverableDescription = c.DeliverableDescription,
            MinFollowers = editData.Requirements?.MinFollowers,
            RequirementNotes = editData.Requirements?.Notes,
            BarterItems = editData.BarterItems.Select(x => new CaseBarterItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Quantity = x.Quantity,
                Note = x.Note
            }).ToList().AsReadOnly(),
            Attachments = editData.Attachments.Select(x => new CaseAttachmentDto
            {
                Id = x.Id,
                FileId = x.FileId,
                FileName = x.File?.FileName ?? string.Empty,
                FileSize = x.File?.FileSize ?? 0,
                MimeType = x.File?.MimeType ?? string.Empty,
                UploadedAt = x.CreatedAt,
                AttachmentType = (short)x.Type
            }).ToList().AsReadOnly(),
            Status = c.Status
        };

        await uow.CommitAsync(ct);
        return Common.Results.Result.Success(dto);
    }

    private static int? TryParseCityId(string? city)
    {
        if (string.IsNullOrWhiteSpace(city)) return null;
        if (int.TryParse(city, out var id)) return id;
        return null;
    }
}
