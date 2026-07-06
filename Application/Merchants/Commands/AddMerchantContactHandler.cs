using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Results;
using Domain.Exceptions;

namespace Application.Merchants.Commands;

public sealed class AddMerchantContactHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo,
    IMerchantContactRepository contactRepo)
{
    public async Task<Result<long>> HandleAsync(AddMerchantContactCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 確認業者存在
        var merchant = await merchantRepo.GetByIdAsync(cmd.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Errors.Merchant.NotFound;

        var newId = await contactRepo.InsertAsync(
            cmd.MerchantId,
            cmd.Name,
            cmd.Phone,
            cmd.Email,
            cmd.Title,
            cmd.Note,
            uow.Session,
            ct);

        await uow.CommitAsync(ct);

        return newId;
    }
}
