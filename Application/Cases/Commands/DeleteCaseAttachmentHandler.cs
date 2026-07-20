using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.FileStorage;
using Common.Results;

namespace Application.Cases.Commands;

public sealed class DeleteCaseAttachmentHandler(
    IUnitOfWork unitOfWork,
    ICaseRepository caseRepo,
    ICaseAttachmentRepository attachmentRepo,
    IFileRepository fileRepo,
    ICaseFileStorage fileStorage)
{
    public async Task<Result> HandleAsync(
        DeleteCaseAttachmentCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var c = await caseRepo.GetByIdAndMerchantAsync(cmd.CaseId, cmd.MerchantId, uow.Session, ct);
        if (c is null)
        {
            return Result.Failure(Common.Errors.Error.NotFound("Case.NotFound", "案件不存在或無權限。"));
        }

        var attachment = await attachmentRepo.GetByIdAsync(cmd.AttachmentId, uow.Session, ct);
        if (attachment is null || attachment.CaseId != cmd.CaseId)
        {
            return Result.Failure(Common.Errors.Error.NotFound("Attachment.NotFound", "附件不存在或無權限。"));
        }

        var file = await fileRepo.GetByIdAsync(attachment.FileId, uow.Session, ct);
        var relativePath = file?.FilePath;

        await attachmentRepo.DeleteAsync(attachment.Id, uow.Session, ct);
        await fileRepo.DeleteAsync(attachment.FileId, uow.Session, ct);

        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            await fileStorage.DeleteAsync(relativePath, ct);
        }

        c.TouchUpdatedAt();
        await caseRepo.UpdateAsync(c, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
