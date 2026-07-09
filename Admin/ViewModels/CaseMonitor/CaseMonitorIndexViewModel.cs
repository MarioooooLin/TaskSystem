using Application.Cases.DTOs;
using Common.Pagination;

namespace Admin.ViewModels.CaseMonitor;

/// <summary>案件監控首頁 ViewModel。</summary>
public sealed class CaseMonitorIndexViewModel
{
    public PagedResult<CaseListItemDto> List { get; init; } = null!;
    public CaseSummaryDto Summary { get; init; } = new();
    public CaseAlertDto Alert { get; init; } = new();
    public CaseListQueryViewModel Query { get; init; } = new();
}
