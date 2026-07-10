using Common.Pagination;
using Domain.Enums;

namespace Application.Disputes.Queries;

public sealed record GetDisputeListQuery(
    string? Keyword,
    DisputeStatus? Status,
    string? DisputeType,
    PageQuery PageQuery);
