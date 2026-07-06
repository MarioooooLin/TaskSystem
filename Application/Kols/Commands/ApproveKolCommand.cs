namespace Application.Kols.Commands;

/// <summary>審核通過 KOL（Pending 或 Rejected → Approved）。</summary>
public sealed record ApproveKolCommand(long KolId);
