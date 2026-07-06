namespace Application.Kols.Commands;

/// <summary>停權 KOL（Approved → Suspended），需填停權原因。</summary>
public sealed record SuspendKolCommand(long KolId, string SuspensionNote);
