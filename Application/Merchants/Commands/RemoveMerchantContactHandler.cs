using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Errors;
using Common.Results;

namespace Application.Merchants.Commands;

public sealed class RemoveMerchantContactHandler(
    IUnitOfWork unitOfWork,
    IMerchantContactRepository contactRepo)
{
    public async Task<Result> HandleAsync(RemoveMerchantContactCommand cmd, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 確認聯絡窗口屬於該業者（防止越權操作）
        var belongs = await contactRepo.BelongsToMerchantAsync(cmd.ContactId, cmd.MerchantId, uow.Session, ct);
        if (!belongs)
            return Result.Failure(Error.NotFound("MerchantContact.NotFound", "聯絡窗口不存在或不屬於此業者。"));

        await contactRepo.DeleteAsync(cmd.ContactId, uow.Session, ct);
        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
