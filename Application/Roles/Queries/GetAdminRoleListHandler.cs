using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Roles.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Roles.Queries;

public sealed class GetAdminRoleListHandler(
    IUnitOfWork unitOfWork,
    IRoleRepository roleRepo)
{
    public async Task<Result<AdminRoleListResultDto>> HandleAsync(
        GetAdminRoleListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var pageQuery = query.ToPageQuery();
        var (items, totalCount) = await roleRepo.GetSystemRoleListAsync(
            query.Keyword,
            query.IsActive,
            query.IsSystemReserved,
            query.HasHighRiskPermission,
            pageQuery,
            uow.Session,
            ct);

        var summary = await roleRepo.GetSystemRoleSummaryAsync(uow.Session, ct);

        await uow.CommitAsync(ct);

        return Result.Success(new AdminRoleListResultDto
        {
            List = new PagedResult<AdminRoleListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount),
            Summary = summary
        });
    }
}
