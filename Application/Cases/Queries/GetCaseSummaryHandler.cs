using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Results;

namespace Application.Cases.Queries;

public sealed class GetCaseSummaryHandler(
    IUnitOfWork unitOfWork,
    ICaseMonitorRepository caseMonitorRepo)
{
    public async Task<Result<(CaseSummaryDto Summary, CaseAlertDto Alert)>> HandleAsync(
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var summary = await caseMonitorRepo.GetSummaryAsync(uow.Session, ct);
        var alert = await caseMonitorRepo.GetAlertAsync(uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<(CaseSummaryDto, CaseAlertDto)>.Success((summary, alert));
    }
}
