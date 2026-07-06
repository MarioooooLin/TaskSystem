using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Merchants.Queries;

public sealed class GetMerchantListHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo)
{
    public async Task<Result<PagedResult<MerchantListItemDto>>> HandleAsync(
        GetMerchantListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();

        var (items, totalCount) = await merchantRepo.GetListAsync(
            query.Keyword,
            query.VerificationStatus,
            pageQuery,
            uow.Session,
            ct);

        var result = new PagedResult<MerchantListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount);

        return Result<PagedResult<MerchantListItemDto>>.Success(result);
    }
}
