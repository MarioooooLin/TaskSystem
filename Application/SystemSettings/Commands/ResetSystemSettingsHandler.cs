using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;

namespace Application.SystemSettings.Commands;

public sealed class ResetSystemSettingsHandler(
    IUnitOfWork unitOfWork,
    ISystemSettingRepository systemSettingRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    public async Task<Result> HandleAsync(
        ResetSystemSettingsCommand cmd,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var changedKeys = await systemSettingRepo.ResetToDefaultsAsync(
            currentUser.UserId,
            cmd.Note,
            uow.Session,
            ct);

        if (changedKeys.Count > 0)
        {
            await activityLogRepo.WriteAsync(
                targetType: "SystemSettings",
                targetId: 0,
                actorUserId: currentUser.UserId,
                action: "ResetSystemSettings",
                note: $"已還原預設值參數：{string.Join(", ", changedKeys)}",
                session: uow.Session,
                ct: ct);
        }

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}
