using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Cases.Queries;

public sealed class GetCaseListHandler(
    IUnitOfWork unitOfWork,
    ICaseMonitorRepository caseMonitorRepo)
{
    public async Task<Result<PagedResult<CaseListItemDto>>> HandleAsync(
        GetCaseListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();

        var (items, totalCount) = await caseMonitorRepo.GetListAsync(
            query.Keyword,
            query.Status,
            query.HasPendingReview,
            query.HasCommission,
            query.DateFrom,
            query.DateTo,
            pageQuery,
            uow.Session,
            ct);

        var result = new PagedResult<CaseListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount);
        return Result<PagedResult<CaseListItemDto>>.Success(result);
    }
}
