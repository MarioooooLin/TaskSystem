using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Merchants.Commands;

public sealed class AdjustMerchantCreditHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo,
    IMerchantCreditWalletRepository creditWalletRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(AdjustMerchantCreditCommand cmd, CancellationToken ct = default)
    {
        if (cmd.Amount <= 0)
            return Result.Failure(Error.Validation("Credit.InvalidAmount", "金額必須大於 0。"));

        // OperationType: 1=Grant  4=Revoke
        if (cmd.OperationType is not (1 or 4))
            return Result.Failure(Error.Validation("Credit.InvalidType", "操作類型無效。"));

        await using var uow = await unitOfWork.BeginAsync(ct);

        var merchant = await merchantRepo.GetByIdAsync(cmd.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Result.Failure(Errors.Merchant.NotFound);

        var wallet = await creditWalletRepo.GetByMerchantIdAsync(cmd.MerchantId, uow.Session, ct)
                     ?? new MerchantCreditWallet { MerchantId = cmd.MerchantId };

        if (cmd.OperationType == 4)
        {
            // 扣回：不可超過可用餘額
            if (cmd.Amount > wallet.AvailableAmount)
                return Result.Failure(Error.Validation("Credit.InsufficientBalance",
                    $"扣回金額 {cmd.Amount:N0} 超過可用折扣金餘額 {wallet.AvailableAmount:N0}。"));

            wallet.AvailableAmount -= cmd.Amount;
            wallet.RevokedAmount   += cmd.Amount;
        }
        else
        {
            // 加值
            wallet.AvailableAmount += cmd.Amount;
        }

        wallet.UpdatedAt = DateTime.UtcNow;
        await creditWalletRepo.UpsertAsync(wallet, uow.Session, ct);

        // 寫入交易流水
        await creditWalletRepo.InsertTransactionAsync(
            merchantId:      cmd.MerchantId,
            type:            cmd.OperationType,
            amount:          cmd.OperationType == 4 ? -cmd.Amount : cmd.Amount,
            relatedCaseId:   null,
            reason:          cmd.Reason,
            note:            cmd.Note,
            createdByUserId: currentUser.UserId,
            session:         uow.Session,
            ct:              ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
