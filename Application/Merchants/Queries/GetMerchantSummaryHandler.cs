using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Common.Results;

namespace Application.Merchants.Queries;

public sealed class GetMerchantSummaryHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo)
{
    public async Task<Result<MerchantSummaryDto>> HandleAsync(
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);
        var summary = await merchantRepo.GetSummaryAsync(uow.Session, ct);
        return Result<MerchantSummaryDto>.Success(summary);
    }
}
