using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Kols.Queries;

public sealed class GetKolReviewListHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo)
{
    public async Task<Result<PagedResult<KolReviewListItemDto>>> HandleAsync(
        GetKolReviewListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();

        var (items, totalCount) = await kolRepo.GetReviewListAsync(
            query.Keyword,
            query.StatusFilter,
            query.Category,
            query.Platform,
            query.SubmittedDate,
            pageQuery,
            uow.Session,
            ct);

        return new PagedResult<KolReviewListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount);
    }
}
