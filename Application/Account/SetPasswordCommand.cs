using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Account;

/// <summary>
/// 透過邀請連結設定後台帳號密碼。
/// </summary>
public sealed record SetPasswordCommand(string Token, string Email, string Password);

/// <summary>
/// 設定密碼處理程序：驗證邀請、雜湊密碼、更新使用者、標記邀請已接受。
/// </summary>
public sealed class SetPasswordHandler(
    IUserRepository userRepo,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task<Result> HandleAsync(
        SetPasswordCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var invitation = await userRepo.GetPendingInvitationByTokenAsync(
            cmd.Token, cmd.Email, uow.Session, ct);

        if (invitation is null)
            return Result.Failure(Errors.Invitation.NotFound);

        if (invitation.Status != InvitationStatus.Pending)
            return Result.Failure(Errors.Invitation.AlreadyAccepted);

        if (invitation.IsExpired())
            return Result.Failure(Errors.Invitation.Expired);

        if (invitation.UserId is null)
            return Result.Failure(Errors.Invitation.NotFound);

        var user = await userRepo.GetByIdAsync(invitation.UserId.Value, uow.Session, ct);
        if (user is null)
            return Result.Failure(Errors.User.NotFound);

        var hash = passwordHasher.Hash(cmd.Password);
        await userRepo.UpdatePasswordAsync(user.Id, hash, uow.Session, ct);
        await userRepo.AcceptInvitationAsync(invitation.Id, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
