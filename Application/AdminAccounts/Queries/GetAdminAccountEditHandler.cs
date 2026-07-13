using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.AdminAccounts.DTOs;
using Common.Results;
using Domain.Exceptions;

namespace Application.AdminAccounts.Queries;

public sealed class GetAdminAccountEditHandler(
    IUnitOfWork unitOfWork,
    IAdminAccountRepository adminAccountRepo)
{
    public async Task<Result<AdminAccountEditDto>> HandleAsync(
        GetAdminAccountEditQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var dto = await adminAccountRepo.GetByIdAsync(query.UserId, uow.Session, ct);
        if (dto is null)
            return Result.Failure<AdminAccountEditDto>(Errors.AdminAccount.NotFound);

        await uow.CommitAsync(ct);
        return Result<AdminAccountEditDto>.Success(dto);
    }
}
