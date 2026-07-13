using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.AdminAccounts.DTOs;
using Common.Results;

namespace Application.AdminAccounts.Queries;

public sealed class GetAdminAccountSummaryHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo)
{
    public async Task<Result<AdminAccountSummaryDto>> HandleAsync(
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var summary = await adminAccountRepo.GetSummaryAsync(uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<AdminAccountSummaryDto>.Success(summary);
    }
}
