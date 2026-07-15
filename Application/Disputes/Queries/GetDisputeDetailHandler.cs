using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Disputes.DTOs;
using Common.Results;

namespace Application.Disputes.Queries;

public sealed class GetDisputeDetailHandler(
    IUnitOfWork unitOfWork,
    IDisputeRepository disputeRepo)
{
    public async Task<Result<DisputeDetailDto>> HandleAsync(
        GetDisputeDetailQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var detail = await disputeRepo.GetDetailAsync(query.DisputeId, uow.Session, ct);

        await uow.CommitAsync(ct);

        if (detail is null)
            return Common.Errors.Error.NotFound("Dispute.NotFound", "找不到該異議資料。");

        return detail;
    }
}
