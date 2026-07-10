using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Disputes.DTOs;
using Common.Results;

namespace Application.Disputes.Queries;

public sealed class GetDisputeSummaryHandler(IUnitOfWork unitOfWork, IDisputeRepository disputeRepo)
{
    public async Task<Result<DisputeSummaryDto>> HandleAsync(CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var summary = await disputeRepo.GetSummaryAsync(uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<DisputeSummaryDto>.Success(summary);
    }
}
