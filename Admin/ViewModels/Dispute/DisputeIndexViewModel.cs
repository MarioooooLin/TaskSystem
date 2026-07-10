using Application.Disputes.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.Dispute;

public sealed class DisputeIndexViewModel
{
    public DisputeSummaryDto Summary { get; set; } = new();
    public PagedResult<DisputeListItemDto> List { get; set; } = PagedResult<DisputeListItemDto>.Empty(1, 20);
    public DisputeListQueryViewModel Query { get; set; } = new();
}
