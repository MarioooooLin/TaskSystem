using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Common.Results;
using Domain.Entities;

namespace Application.SystemSettings.Queries;

public sealed class GetActiveLanguagesHandler(
    IUnitOfWork unitOfWork,
    ILanguageRepository languageRepo)
{
    public async Task<Result<IReadOnlyList<Language>>> HandleAsync(
        GetActiveLanguagesQuery _, CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);
        var languages = await languageRepo.GetActiveAsync(uow.Session, ct);
        await uow.CommitAsync(ct);
        return Result<IReadOnlyList<Language>>.Success(languages);
    }
}
