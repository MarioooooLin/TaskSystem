using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Cases.Queries;

public sealed class GetMerchantCaseListHandler(
    IUnitOfWork unitOfWork,
    ICaseMonitorRepository caseMonitorRepo)
{
    public async Task<Result<PagedResult<MerchantCaseListItemDto>>> HandleAsync(
        GetMerchantCaseListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();

        var (items, totalCount) = await caseMonitorRepo.GetMerchantListAsync(
            query.MerchantId,
            query.Keyword,
            query.Status,
            query.ClosedOnly,
            query.RewardTypeFilter,
            query.Platform,
            query.DateFrom,
            query.DateTo,
            pageQuery,
            uow.Session,
            ct);

        var result = new PagedResult<MerchantCaseListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount);
        await uow.CommitAsync(ct);
        return Result<PagedResult<MerchantCaseListItemDto>>.Success(result);
    }
}
