using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ILanguageRepository
{
    Task<IReadOnlyList<Language>> GetActiveAsync(IDbSession session, CancellationToken ct = default);
}
