using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.AdminAccounts.DTOs;
using Common.Results;

namespace Application.AdminAccounts.Queries;

public sealed class GetRecentAdminAccountLogsHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo)
{
    public async Task<Result<IReadOnlyList<AdminAccountLogItemDto>>> HandleAsync(
        int count = 10,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var logs = await adminAccountRepo.GetRecentLogsAsync(count, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<IReadOnlyList<AdminAccountLogItemDto>>.Success(logs);
    }
}
