using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.AdminAccounts.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.AdminAccounts.Queries;

public sealed class GetAdminAccountListHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo)
{
    public async Task<Result<PagedResult<AdminAccountListItemDto>>> HandleAsync(
        GetAdminAccountListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();
        var (items, totalCount) = await adminAccountRepo.GetListAsync(
            query.Keyword,
            query.Status,
            query.Department,
            query.RoleId,
            pageQuery,
            uow.Session,
            ct);

        await uow.CommitAsync(ct);
        return Result<PagedResult<AdminAccountListItemDto>>.Success(
            new PagedResult<AdminAccountListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount));
    }
}
