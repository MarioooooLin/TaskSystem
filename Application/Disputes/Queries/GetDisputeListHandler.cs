using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Disputes.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Disputes.Queries;

public sealed class GetDisputeListHandler(
    IUnitOfWork unitOfWork,
    IDisputeRepository disputeRepo)
{
    public async Task<Result<PagedResult<DisputeListItemDto>>> HandleAsync(
        GetDisputeListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var (items, totalCount) = await disputeRepo.GetListAsync(
            query.Keyword,
            query.Status,
            query.DisputeType,
            query.PageQuery,
            uow.Session,
            ct);

        var result = new PagedResult<DisputeListItemDto>(
            items,
            query.PageQuery.Page,
            query.PageQuery.PageSize,
            totalCount);

        await uow.CommitAsync(ct);
        return Result<PagedResult<DisputeListItemDto>>.Success(result);
    }
}
