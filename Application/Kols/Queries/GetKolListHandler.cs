using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Kols.Queries;

public sealed class GetKolListHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo)
{
    public async Task<Result<PagedResult<KolListItemDto>>> HandleAsync(
        GetKolListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();

        var (items, totalCount) = await kolRepo.GetListAsync(
            query.Keyword,
            query.VerificationStatus,
            query.Category,
            query.Platform,
            query.HasBankAccount,
            pageQuery,
            uow.Session,
            ct);

        return new PagedResult<KolListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount);
    }
}
