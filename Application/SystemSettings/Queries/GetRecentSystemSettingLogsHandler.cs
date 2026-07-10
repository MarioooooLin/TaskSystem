using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.SystemSettings.DTOs;
using Common.Results;

namespace Application.SystemSettings.Queries;

public sealed class GetRecentSystemSettingLogsHandler(
    IUnitOfWork unitOfWork,
    ISystemSettingRepository systemSettingRepo)
{
    public async Task<Result<IReadOnlyList<SystemSettingLogDto>>> HandleAsync(
        GetRecentSystemSettingLogsQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var logs = await systemSettingRepo.GetRecentLogsAsync(query.Count, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<IReadOnlyList<SystemSettingLogDto>>.Success(logs);
    }
}
