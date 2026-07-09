using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Dashboard.DTOs;
using Common.Results;

namespace Application.Dashboard.Queries;

public sealed class GetDashboardHandler(
    IUnitOfWork unitOfWork,
    IDashboardRepository dashboardRepo)
{
    public async Task<Result<DashboardDto>> HandleAsync(
        GetDashboardQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var dashboard = await dashboardRepo.GetDashboardAsync(query.TopK, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<DashboardDto>.Success(dashboard);
    }
}
