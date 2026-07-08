using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Common.Results;

namespace Application.Kols.Queries;

public sealed class GetKolReviewSummaryHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo)
{
    public async Task<Result<KolReviewSummaryDto>> HandleAsync(CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);
        var summary = await kolRepo.GetReviewSummaryAsync(uow.Session, ct);
        return Result<KolReviewSummaryDto>.Success(summary);
    }
}
