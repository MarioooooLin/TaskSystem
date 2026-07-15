using Application.Abstractions.Persistence;
using Application.Disputes.DTOs;
using Common.Pagination;
using Domain.Enums;

namespace Application.Abstractions.Repositories;

public interface IDisputeRepository
{
    Task<(IReadOnlyList<DisputeListItemDto> Items, int TotalCount)> GetListAsync(
        string? keyword,
        DisputeStatus? status,
        string? disputeType,
        PageQuery pageQuery,
        IDbSession session,
        CancellationToken ct = default);

    Task<DisputeSummaryDto> GetSummaryAsync(
        IDbSession session,
        CancellationToken ct = default);

    Task<DisputeDetailDto?> GetDetailAsync(
        long disputeId,
        IDbSession session,
        CancellationToken ct = default);

    Task<bool> ResolveAsync(
        long disputeId,
        DisputeStatus status,
        long resolvedByAdminId,
        string resolutionNote,
        IDbSession session,
        CancellationToken ct = default);
}
