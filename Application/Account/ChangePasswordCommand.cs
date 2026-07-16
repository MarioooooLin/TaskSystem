using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Exceptions;

namespace Application.Account;

/// <summary>
/// 變更目前登入者密碼。
/// </summary>
public sealed record ChangePasswordCommand(
    long UserId,
    string CurrentPassword,
    string NewPassword);

/// <summary>
/// 變更密碼處理程序：驗證目前密碼、雜湊新密碼後更新。
/// </summary>
public sealed class ChangePasswordHandler(
    IUserRepository userRepo,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        ChangePasswordCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var user = await userRepo.GetByIdAsync(cmd.UserId, uow.Session, ct);
        if (user is null)
            return Result.Failure(Errors.User.NotFound);

        if (user.PasswordHash is null ||
            !passwordHasher.Verify(cmd.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(Errors.User.InvalidCredentials);
        }

        var hash = passwordHasher.Hash(cmd.NewPassword);
        await userRepo.UpdatePasswordAsync(user.Id, hash, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
