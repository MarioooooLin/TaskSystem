using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Account;

/// <summary>
/// 驗證邀請連結是否有效。
/// </summary>
public sealed record ValidateInvitationTokenQuery(string Token, string Email);

/// <summary>
/// 驗證邀請連結並回傳設定密碼頁面所需資訊。
/// </summary>
public sealed class ValidateInvitationTokenHandler(
    IUserRepository userRepo,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<SetPasswordInitDto>> HandleAsync(
        ValidateInvitationTokenQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var invitation = await userRepo.GetPendingInvitationByTokenAsync(
            query.Token, query.Email, uow.Session, ct);

        if (invitation is null)
            return Errors.Invitation.NotFound;

        if (invitation.Status != InvitationStatus.Pending)
            return Errors.Invitation.AlreadyAccepted;

        if (invitation.IsExpired())
            return Errors.Invitation.Expired;

        if (invitation.UserId is null)
            return Errors.Invitation.NotFound;

        var user = await userRepo.GetByIdAsync(invitation.UserId.Value, uow.Session, ct);
        if (user is null)
            return Errors.User.NotFound;

        return new SetPasswordInitDto(user.Name, user.Email);
    }
}
