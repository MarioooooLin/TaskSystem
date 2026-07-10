using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Finance.DTOs;
using Common.Pagination;
using Common.Results;

namespace Application.Finance.Queries;

public sealed class GetFinanceListHandler(
    IUnitOfWork unitOfWork,
    IFinanceRepository financeRepo)
{
    public async Task<Result<(FinanceSummaryDto Summary, PagedResult<FinanceListItemDto> List)>> HandleAsync(
        GetFinanceListQuery query,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        // 預設統計期間為當月（台北時區）
        var (dateFrom, dateTo) = ResolveMonthRange(query.DateFrom, query.DateTo);

        var pageQuery = query.ToPageQuery();

        var summary = await financeRepo.GetSummaryAsync(dateFrom, dateTo, uow.Session, ct);
        var (items, totalCount) = await financeRepo.GetListAsync(
            query.Keyword,
            query.Status,
            dateFrom,
            dateTo,
            pageQuery,
            uow.Session,
            ct);

        var list = new PagedResult<FinanceListItemDto>(items, pageQuery.Page, pageQuery.PageSize, totalCount);

        await uow.CommitAsync(ct);
        return Result<(FinanceSummaryDto Summary, PagedResult<FinanceListItemDto> List)>.Success((summary, list));
    }

    private static (DateTime From, DateTime To) ResolveMonthRange(DateTime? dateFrom, DateTime? dateTo)
    {
        if (dateFrom.HasValue && dateTo.HasValue)
            return (dateFrom.Value.Date, dateTo.Value.Date.AddDays(1).AddTicks(-1));

        var taipei = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, taipei);
        var from = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var to = from.AddMonths(1).AddTicks(-1);
        return (from, to);
    }
}
