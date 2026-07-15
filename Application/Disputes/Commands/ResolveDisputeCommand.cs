using Domain.Enums;

namespace Application.Disputes.Commands;

public sealed record ResolveDisputeCommand(
    long DisputeId,
    DisputeStatus Resolution,
    string ResolutionNote);
