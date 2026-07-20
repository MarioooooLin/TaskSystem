using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Results;
using Common.Security;
using Domain.Exceptions;

namespace Application.Merchants.Commands;

public sealed class RedeemMerchantImpersonationTicketHandler(
    IUnitOfWork unitOfWork,
    IMerchantImpersonationTicketRepository ticketRepo,
    IMerchantRepository merchantRepo,
    IUserRepository userRepo,
    IActivityLogRepository activityLogRepo)
{
    public async Task<Result<RedeemMerchantImpersonationTicketResult>> HandleAsync(
        RedeemMerchantImpersonationTicketCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var tokenHash = ImpersonationTokenHelper.ComputeHash(cmd.Token);
        var ticket = await ticketRepo.GetByTokenHashAsync(tokenHash, uow.Session, ct);

        if (ticket is null)
            return Errors.Impersonation.InvalidToken;

        // 過期或已使用均回傳同一錯誤，避免洩漏票證狀態
        if (ticket.UsedAtUtc.HasValue || ticket.ExpiresAtUtc <= DateTime.UtcNow)
            return Errors.Impersonation.InvalidToken;

        var merchant = await merchantRepo.GetByIdAsync(ticket.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Errors.Impersonation.InvalidToken;

        var admin = await userRepo.GetByIdAsync(ticket.AdminUserId, uow.Session, ct);
        if (admin is null)
            return Errors.Impersonation.InvalidToken;

        var redeemed = await ticketRepo.TryRedeemAsync(ticket.Id, DateTime.UtcNow, uow.Session, ct);
        if (!redeemed)
            return Errors.Impersonation.InvalidToken;

        await activityLogRepo.WriteAsync(
            targetType: "MerchantImpersonationTickets",
            targetId: ticket.Id,
            actorUserId: admin.Id,
            action: "ImpersonationTicketRedeemed",
            note: $"MerchantId={merchant.Id}",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);

        return Result<RedeemMerchantImpersonationTicketResult>.Success(
            new RedeemMerchantImpersonationTicketResult(
                ticket.Id,
                merchant.Id,
                merchant.CompanyName,
                admin.Id,
                admin.Name));
    }
}
