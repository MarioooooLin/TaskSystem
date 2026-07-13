using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.AdminAccounts.DTOs;
using Common.Results;

namespace Application.AdminAccounts.Queries;

public sealed class GetAdminRoleOptionsHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo)
{
    public async Task<Result<IReadOnlyList<AdminRoleOptionDto>>> HandleAsync(
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var roles = await adminAccountRepo.GetActiveSystemRolesAsync(uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result<IReadOnlyList<AdminRoleOptionDto>>.Success(roles);
    }
}
