using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Results;
using Domain.Exceptions;

namespace Application.Account;

/// <summary>
/// 查詢個人帳號資料。
/// </summary>
public sealed record ProfileQuery(long UserId);

/// <summary>
/// 取得個人帳號頁面資料。
/// </summary>
public sealed class ProfileHandler(IUserRepository userRepo, IUnitOfWork unitOfWork)
{
    public async Task<Result<ProfileDto>> HandleAsync(
        ProfileQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var user = await userRepo.GetByIdAsync(query.UserId, uow.Session, ct);
        if (user is null)
            return Errors.User.NotFound;

        var roleNames = await userRepo.GetRoleNamesByUserIdAsync(query.UserId, uow.Session, ct);

        return new ProfileDto(
            user.Name,
            user.Email,
            string.Join("、", roleNames));
    }
}
