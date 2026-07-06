using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Results;
using Domain.Exceptions;

namespace Application.Merchants.Commands;

public sealed class UpdateMerchantHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo)
{
    public async Task<Result> HandleAsync(UpdateMerchantCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var merchant = await merchantRepo.GetByIdAsync(cmd.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Result.Failure(Errors.Merchant.NotFound);

        // 更新欄位
        merchant.CompanyName = cmd.CompanyName;
        merchant.EnglishName = cmd.EnglishName;
        merchant.TaxId = cmd.TaxId;
        merchant.IndustryType = cmd.IndustryType;
        merchant.ContactName = cmd.ContactName;
        merchant.Phone = cmd.Phone;
        merchant.Fax = cmd.Fax;
        merchant.CompanyEmail = cmd.CompanyEmail;
        merchant.Website = cmd.Website;
        merchant.Address = cmd.Address;
        merchant.EstablishedDate = cmd.EstablishedDate;
        merchant.UpdatedAt = DateTime.UtcNow;

        await merchantRepo.UpdateAsync(merchant, uow.Session, ct);
        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
