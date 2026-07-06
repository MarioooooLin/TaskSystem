namespace Application.Kols.Commands;

/// <summary>解除停權 KOL（Suspended → Approved）。</summary>
public sealed record UnsuspendKolCommand(long KolId);
