using Application.Abstractions.Persistence;
using Application.Kols.DTOs;

namespace Application.Abstractions.Repositories;

public interface IKolSocialAccountRepository
{
    Task<IReadOnlyList<KolSocialAccountDto>> GetByKolIdAsync(
        long kolId,
        IDbSession session,
        CancellationToken ct = default);
}
