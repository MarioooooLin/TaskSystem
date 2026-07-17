using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Results;

namespace Application.Cases.Queries;

public sealed class GetMerchantCaseSummaryHandler(
    IUnitOfWork unitOfWork,
    ICaseMonitorRepository caseMonitorRepo)
{
    public async Task<Result<MerchantCaseSummaryDto>> HandleAsync(
        GetMerchantCaseSummaryQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var summary = await caseMonitorRepo.GetMerchantSummaryAsync(query.MerchantId, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<MerchantCaseSummaryDto>.Success(summary);
    }
}
