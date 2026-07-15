using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Errors;
using Common.Results;
using Domain.Enums;

namespace Application.Disputes.Commands;

public sealed class ResolveDisputeHandler(
    IUnitOfWork unitOfWork,
    IDisputeRepository disputeRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        ResolveDisputeCommand command,
        CancellationToken ct = default)
    {
        if (!currentUser.IsAuthenticated)
            return Result.Failure(Error.Forbidden("Dispute.Unauthorized", "未登入，無法處理異議。"));

        if (!currentUser.HasPermission("Admin.Dispute.Resolve"))
            return Result.Failure(Error.Forbidden("Dispute.NoPermission", "您沒有處理異議的權限。"));

        var validResolutions = new[]
        {
            DisputeStatus.ResolvedForMerchant,
            DisputeStatus.ResolvedForKol,
            DisputeStatus.ResolvedCompromise,
            DisputeStatus.UnderReview
        };

        if (!validResolutions.Contains(command.Resolution))
            return Result.Failure(Error.Validation("Dispute.InvalidResolution", "無效的處理結果。"));

        if (string.IsNullOrWhiteSpace(command.ResolutionNote))
            return Result.Failure(Error.Validation("Dispute.NoteRequired", "平台處理意見為必填。"));

        await using var uow = await unitOfWork.BeginAsync(ct);

        var detail = await disputeRepo.GetDetailAsync(command.DisputeId, uow.Session, ct);
        if (detail is null)
            return Result.Failure(Error.NotFound("Dispute.NotFound", "找不到該異議資料。"));

        if (detail.Status is DisputeStatus.ResolvedForMerchant
            or DisputeStatus.ResolvedForKol
            or DisputeStatus.ResolvedCompromise
            or DisputeStatus.Cancelled)
        {
            return Result.Failure(Error.Conflict("Dispute.AlreadyResolved", "該異議已結案，無法再次處理。"));
        }

        var note = command.ResolutionNote.Trim();
        var resolved = await disputeRepo.ResolveAsync(
            command.DisputeId,
            command.Resolution,
            currentUser.UserId,
            note,
            uow.Session,
            ct);

        if (!resolved)
            return Result.Failure(Error.Problem("Dispute.ResolveFailed", "更新異議狀態失敗。"));

        await activityLogRepo.WriteAsync(
            "Disputes",
            command.DisputeId,
            currentUser.UserId,
            "DisputeResolved",
            $"處理結果：{command.Resolution}；意見：{note}",
            uow.Session,
            ct);

        await uow.CommitAsync(ct);

        return Result.Success();
    }
}
