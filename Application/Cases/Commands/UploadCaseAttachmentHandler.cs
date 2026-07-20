using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.FileStorage;
using Common.Results;
using Domain.Entities;

namespace Application.Cases.Commands;

public sealed class UploadCaseAttachmentHandler(
    IUnitOfWork unitOfWork,
    ICaseRepository caseRepo,
    ICaseAttachmentRepository attachmentRepo,
    IFileRepository fileRepo,
    ICaseFileStorage fileStorage)
{
    private const long MaxTotalSizeBytes = 50L * 1024 * 1024; // 50MB

    public async Task<Result<long>> HandleAsync(
        UploadCaseAttachmentCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var c = await caseRepo.GetByIdAndMerchantAsync(cmd.CaseId, cmd.MerchantId, uow.Session, ct);
        if (c is null)
        {
            return Result.Failure<long>(Common.Errors.Error.NotFound("Case.NotFound", "案件不存在或無權限。"));
        }

        if (cmd.FileSize <= 0)
        {
            return Result.Failure<long>(Common.Errors.Error.Validation("File.Empty", "上傳檔案不得為空。"));
        }

        var currentTotal = await attachmentRepo.CountTotalSizeByCaseAsync(cmd.CaseId, uow.Session, ct);
        if (currentTotal + cmd.FileSize > MaxTotalSizeBytes)
        {
            return Result.Failure<long>(Common.Errors.Error.Validation("File.SizeExceeded", "附件總大小不得超過 50MB。"));
        }

        // 儲存實體檔案
        var storageInfo = await fileStorage.SaveAsync(
            cmd.FileStream,
            cmd.FileName,
            cmd.ContentType,
            cmd.FileSize,
            cmd.CurrentUserId,
            ct);

        // 寫入 Files 中繼資料
        var fileEntity = new FileEntity
        {
            UploadedByUserId = cmd.CurrentUserId,
            FileName = storageInfo.FileName,
            FilePath = storageInfo.RelativePath,
            FileSize = storageInfo.FileSize,
            MimeType = storageInfo.MimeType,
            CreatedAt = DateTime.UtcNow
        };
        var fileId = await fileRepo.InsertAsync(fileEntity, uow.Session, ct);
        fileEntity.Id = fileId;

        // 關聯案件
        var attachment = new CaseAttachment
        {
            CaseId = cmd.CaseId,
            FileId = fileId,
            Type = cmd.AttachmentType,
            CreatedAt = DateTime.UtcNow
        };
        var attachmentId = await attachmentRepo.InsertAsync(attachment, uow.Session, ct);

        c.TouchUpdatedAt();
        await caseRepo.UpdateAsync(c, uow.Session, ct);

        await uow.CommitAsync(ct);
        return Result.Success(attachmentId);
    }
}
