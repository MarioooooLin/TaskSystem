using Application.Disputes.DTOs;

namespace Admin.ViewModels.Dispute;

public sealed class DisputeDetailViewModel
{
    public DisputeDetailDto Detail { get; set; } = new();
    public ResolveDisputeViewModel Resolve { get; set; } = new();
}
