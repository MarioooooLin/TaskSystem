using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Application.Merchants.Options;
using Common.Results;
using Common.Security;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Options;

namespace Application.Merchants.Commands;

public sealed class CreateMerchantImpersonationTicketHandler(
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IMerchantRepository merchantRepo,
    IMerchantImpersonationTicketRepository ticketRepo,
    IActivityLogRepository activityLogRepo,
    IOptions<MerchantImpersonationOptions> options)
{
    public async Task<Result<CreateMerchantImpersonationTicketResult>> HandleAsync(
        CreateMerchantImpersonationTicketCommand cmd,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated)
            return Errors.User.Forbidden;

        if (!currentUser.HasPermission("Admin.Merchant.Impersonate"))
            return Errors.Impersonation.PermissionDenied;

        await using var uow = await unitOfWork.BeginAsync(ct);

        var merchant = await merchantRepo.GetByIdAsync(cmd.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Errors.Impersonation.MerchantNotFound;

        if (merchant.VerificationStatus != VerificationStatus.Approved)
            return Errors.Impersonation.MerchantNotApproved;

        var plainToken = ImpersonationTokenHelper.GenerateToken();
        var tokenHash = ImpersonationTokenHelper.ComputeHash(plainToken);
        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(options.Value.TicketLifetimeSeconds);

        var ticket = new MerchantImpersonationTicket
        {
            TokenHash = tokenHash,
            MerchantId = merchant.Id,
            AdminUserId = currentUser.UserId,
            CreatedAtUtc = now,
            ExpiresAtUtc = expiresAt,
            CreatedIp = cmd.CreatedIp,
            UserAgent = cmd.UserAgent,
        };

        var ticketId = await ticketRepo.InsertAsync(ticket, uow.Session, ct);

        await activityLogRepo.WriteAsync(
            targetType: "MerchantImpersonationTickets",
            targetId: ticketId,
            actorUserId: currentUser.UserId,
            action: "ImpersonationTicketCreated",
            note: $"MerchantId={merchant.Id}",
            session: uow.Session,
            ct: ct);

        await uow.CommitAsync(ct);

        return Result<CreateMerchantImpersonationTicketResult>.Success(
            new CreateMerchantImpersonationTicketResult(
                ticketId,
                plainToken,
                merchant.Id,
                merchant.CompanyName,
                currentUser.UserId,
                expiresAt));
    }
}
