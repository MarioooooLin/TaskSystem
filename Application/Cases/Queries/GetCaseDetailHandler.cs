using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Cases.DTOs;
using Common.Errors;
using Common.Results;

namespace Application.Cases.Queries;

public sealed class GetCaseDetailHandler(
    IUnitOfWork unitOfWork,
    ICaseMonitorRepository caseMonitorRepo)
{
    public async Task<Result<CaseDetailDto>> HandleAsync(
        GetCaseDetailQuery query, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var detail = await caseMonitorRepo.GetDetailAsync(query.CaseId, uow.Session, ct);
        if (detail is null)
            return Result.Failure<CaseDetailDto>(Error.NotFound("Case.NotFound", "找不到指定案件。"));

        await uow.CommitAsync(ct);
        return Result<CaseDetailDto>.Success(detail);
    }
}
