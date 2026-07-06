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

        // 2. 平行查詢子集合
        var socialTask = socialRepo.GetByKolIdAsync(query.KolId, uow.Session, ct);
        var bankTask = bankRepo.GetByKolIdAsync(query.KolId, uow.Session, ct);
        var statsTask = statsRepo.GetStatsByKolIdAsync(query.KolId, uow.Session, ct);
        var earningsTask = statsRepo.GetEarningsSummaryAsync(query.KolId, uow.Session, ct);
        var tasksTask = statsRepo.GetRecentTasksAsync(query.KolId, RecentCount, uow.Session, ct);
        var logsTask = statsRepo.GetRecentActivityLogsAsync(query.KolId, RecentCount, uow.Session, ct);
        var categoriesTask = statsRepo.GetCategoriesAsync(query.KolId, uow.Session, ct);

        await Task.WhenAll(socialTask, bankTask, statsTask, earningsTask, tasksTask, logsTask, categoriesTask);

        var stats = await statsTask;

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
            Categories = await categoriesTask,
            CreatedAt = baseDto.CreatedAt,

            VerificationStatus = baseDto.VerificationStatus,
            VerifiedAt = baseDto.VerifiedAt,
            VerifiedByAdminName = baseDto.VerifiedByAdminName,
            RejectionNote = baseDto.RejectionNote,
            SuspensionNote = baseDto.SuspensionNote,

            TotalFollowers = baseDto.FollowersCount ?? 0,
            TaskCount = stats.TaskCount,
            CompletedTaskCount = stats.CompletedTaskCount,
            PendingReviewCount = stats.PendingReviewCount,
            DisputeCount = stats.DisputeCount,

            SocialAccounts = await socialTask,
            BankAccount = await bankTask,
            RecentTasks = await tasksTask,
            Earnings = await earningsTask,
            RecentActivityLogs = await logsTask,
        };

        return detail;
    }
}
