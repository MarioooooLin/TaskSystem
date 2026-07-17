using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Merchants.DTOs;
using Common.Results;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Merchants.Queries;

/// <summary>
/// 業者端首頁儀表板 Use Case。
/// </summary>
public sealed class GetMerchantDashboardHandler(
    IUnitOfWork unitOfWork,
    IMerchantRepository merchantRepo,
    IMerchantWalletRepository walletRepo,
    IMerchantStatsRepository statsRepo)
{
    public async Task<Result<MerchantDashboardDto>> HandleAsync(
        GetMerchantDashboardQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 1. 確認業者存在且已通過審核
        var merchant = await merchantRepo.GetByIdAsync(query.MerchantId, uow.Session, ct);
        if (merchant is null)
            return Errors.Merchant.NotFound;

        if (merchant.VerificationStatus != VerificationStatus.Approved)
            return Errors.Merchant.NotApproved;

        // 2. 錢包資料
        var wallet = await walletRepo.GetByMerchantIdAsync(query.MerchantId, uow.Session, ct);
        var availableAmount = wallet?.AvailableAmount ?? 0;
        var frozenAmount = wallet?.FrozenAmount ?? 0;

        // 3. 案件狀態統計
        var cases = await statsRepo.GetRecentCasesAsync(query.MerchantId, int.MaxValue, uow.Session, ct);
        var statusCounts = BuildStatusCounts(cases);

        // 4. 待辦事項（只取最近 5 筆有明確待辦意義的案件）
        var todos = BuildTodos(cases.Take(5).ToList());

        // 5. 最新案件（最近 5 筆）
        var recentCases = BuildRecentCases(cases.Take(5).ToList());

        var dto = new MerchantDashboardDto
        {
            CompanyName = merchant.CompanyName,
            StatusLabel = "啟用中",
            Wallet = new MerchantDashboardWalletDto
            {
                AvailableAmount = availableAmount,
                FrozenAmount = frozenAmount,
                TotalAmount = availableAmount + frozenAmount
            },
            StatusCounts = statusCounts,
            Todos = todos,
            RecentCases = recentCases,
            PendingReviewCount = cases.Count(c => c.Status == CaseStatus.Completed)
        };

        return Result<MerchantDashboardDto>.Success(dto);
    }

    private static IReadOnlyList<MerchantDashboardStatusCountDto> BuildStatusCounts(
        IReadOnlyList<MerchantCaseSummaryDto> cases)
    {
        return
        [
            new MerchantDashboardStatusCountDto
            {
                Category = CaseStatusCategory.Draft,
                Label = "草稿",
                IconUrl = "/images/icon-draft.svg",
                Count = cases.Count(c => c.Status == CaseStatus.Draft)
            },
            new MerchantDashboardStatusCountDto
            {
                Category = CaseStatusCategory.PendingReview,
                Label = "待審核",
                IconUrl = "/images/icon-review.svg",
                Count = cases.Count(c => c.Status == CaseStatus.RecruitmentClosed)
            },
            new MerchantDashboardStatusCountDto
            {
                Category = CaseStatusCategory.Recruiting,
                Label = "招募中",
                IconUrl = "/images/icon-recruit.svg",
                Count = cases.Count(c => c.Status == CaseStatus.Recruiting)
            },
            new MerchantDashboardStatusCountDto
            {
                Category = CaseStatusCategory.InProgress,
                Label = "執行中",
                IconUrl = "/images/icon-progress.svg",
                Count = cases.Count(c => c.Status == CaseStatus.InProgress)
            },
            new MerchantDashboardStatusCountDto
            {
                Category = CaseStatusCategory.PendingAcceptance,
                Label = "待驗收",
                IconUrl = "/images/icon-accepting.svg",
                Count = cases.Count(c => c.Status == CaseStatus.Completed)
            },
            new MerchantDashboardStatusCountDto
            {
                Category = CaseStatusCategory.Closed,
                Label = "已結案",
                IconUrl = "/images/icon-closed.svg",
                Count = cases.Count(c => c.Status is CaseStatus.Settled or CaseStatus.Cancelled)
            }
        ];
    }

    private static IReadOnlyList<MerchantDashboardTodoDto> BuildTodos(
        IReadOnlyList<MerchantCaseSummaryDto> cases)
    {
        var result = new List<MerchantDashboardTodoDto>();

        foreach (var c in cases)
        {
            // 待驗收
            if (c.Status == CaseStatus.Completed)
            {
                result.Add(new MerchantDashboardTodoDto
                {
                    CaseId = c.CaseId,
                    TodoType = "成果待驗收",
                    Title = c.Title,
                    Status = "待驗收",
                    StatusCssClass = "accepting",
                    CreatedAt = c.CreatedAt,
                    ActionText = "查看",
                    ActionUrl = $"/Case/Detail/{c.CaseId}"
                });
            }

            // 餘額可能不足：簡化判斷為招募中但已無可用餘額（實際應用應檢查案件預算）
            if (c.Status == CaseStatus.Recruiting)
            {
                result.Add(new MerchantDashboardTodoDto
                {
                    CaseId = c.CaseId,
                    TodoType = "餘額可能不足",
                    Title = c.Title,
                    Status = "需儲值",
                    StatusCssClass = "deposit",
                    CreatedAt = c.CreatedAt,
                    ActionText = "前往儲值",
                    ActionUrl = "/Wallet/Deposit"
                });
            }
        }

        return result.Take(5).ToList();
    }

    private static IReadOnlyList<MerchantDashboardRecentCaseDto> BuildRecentCases(
        IReadOnlyList<MerchantCaseSummaryDto> cases)
    {
        return cases.Select(c => new MerchantDashboardRecentCaseDto
        {
            CaseId = c.CaseId,
            TypeLabel = c.Status switch
            {
                CaseStatus.Completed => "成果待驗收",
                CaseStatus.Recruiting => "餘額可能不足",
                _ => "最新案件"
            },
            Title = c.Title,
            Status = MapStatusText(c.Status),
            StatusCssClass = MapStatusCssClass(c.Status),
            CreatedAt = c.CreatedAt,
            ActionText = "查看",
            ActionUrl = $"/Case/Detail/{c.CaseId}"
        }).ToList();
    }

    private static string MapStatusText(CaseStatus status) => status switch
    {
        CaseStatus.Draft => "草稿",
        CaseStatus.Recruiting => "招募中",
        CaseStatus.RecruitmentClosed => "招募截止",
        CaseStatus.InProgress => "執行中",
        CaseStatus.Completed => "待驗收",
        CaseStatus.Settled => "已結案",
        CaseStatus.Cancelled => "已取消",
        _ => "未知"
    };

    private static string MapStatusCssClass(CaseStatus status) => status switch
    {
        CaseStatus.Draft => "draft",
        CaseStatus.Recruiting => "recruit",
        CaseStatus.RecruitmentClosed => "review",
        CaseStatus.InProgress => "progress",
        CaseStatus.Completed => "accepting",
        CaseStatus.Settled => "closed",
        CaseStatus.Cancelled => "closed",
        _ => string.Empty
    };
}
