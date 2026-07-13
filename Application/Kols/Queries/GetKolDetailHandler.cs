using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Kols.DTOs;
using Common.Results;
using Domain.Exceptions;

namespace Application.Kols.Queries;

public sealed class GetKolDetailHandler(
    IUnitOfWork unitOfWork,
    IKolRepository kolRepo,
    IKolSocialAccountRepository socialRepo,
    IKolBankAccountRepository bankRepo,
    IKolStatsRepository statsRepo)
{
    private const int RecentCount = 10;

    public async Task<Result<KolDetailDto>> HandleAsync(
        GetKolDetailQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 1. 基本資料
        var baseDto = await kolRepo.GetDetailBaseAsync(query.KolId, uow.Session, ct);
        if (baseDto is null)
            return Errors.Kol.NotFound;

        // 2. 依序查詢子集合，避免同一連線同時開啟多個 DataReader。
        var socialAccounts = await socialRepo.GetByKolIdAsync(query.KolId, uow.Session, ct);
        var bankAccount = await bankRepo.GetByKolIdAsync(query.KolId, uow.Session, ct);
        var stats = await statsRepo.GetStatsByKolIdAsync(query.KolId, uow.Session, ct);
        var earnings = await statsRepo.GetEarningsSummaryAsync(query.KolId, uow.Session, ct);
        var tasks = await statsRepo.GetRecentTasksAsync(query.KolId, RecentCount, uow.Session, ct);
        var logs = await statsRepo.GetRecentActivityLogsAsync(query.KolId, RecentCount, uow.Session, ct);
        var categories = await statsRepo.GetCategoriesAsync(query.KolId, uow.Session, ct);

        // 3. 組裝 DTO
        var detail = new KolDetailDto
        {
            KolId = baseDto.KolId,
            UserId = baseDto.UserId,
            DisplayName = baseDto.DisplayName,
            RealName = baseDto.RealName,
            UserEmail = baseDto.UserEmail,
            Phone = baseDto.Phone,
            LineContactId = baseDto.LineContactId,
            Intro = baseDto.Intro,
            AcceptsCash = baseDto.AcceptsCash,
            AcceptsBarter = baseDto.AcceptsBarter,
            AcceptsCommission = baseDto.AcceptsCommission,
            Categories = categories,
            CreatedAt = baseDto.CreatedAt,

            VerificationStatus = baseDto.VerificationStatus,
            VerifiedAt = baseDto.VerifiedAt,
            VerifiedByAdminName = baseDto.VerifiedByAdminName,
            RejectionNote = baseDto.RejectionNote,
            SuspensionNote = baseDto.SuspensionNote,
            SubmittedAt = baseDto.UpdatedAt,

            TotalFollowers = baseDto.FollowersCount ?? 0,
            TaskCount = stats.TaskCount,
            CompletedTaskCount = stats.CompletedTaskCount,
            PendingReviewCount = stats.PendingReviewCount,
            DisputeCount = stats.DisputeCount,
            AbandonedTaskCount = stats.AbandonedTaskCount,

            SocialAccounts = socialAccounts,
            BankAccount = bankAccount,
            RecentTasks = tasks,
            Earnings = earnings,
            RecentActivityLogs = logs,
        };

        return detail;
    }
}
