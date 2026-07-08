using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Common.Results;

namespace Application.Kols.Queries;

public sealed class GetKolSummaryHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo)
{
    public async Task<Result<KolSummaryDto>> HandleAsync(
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);
        var summary = await kolRepo.GetSummaryAsync(uow.Session, ct);
        return Result<KolSummaryDto>.Success(summary);
    }
}
