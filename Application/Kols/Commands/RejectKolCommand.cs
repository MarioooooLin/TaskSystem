namespace Application.Kols.Commands;

/// <summary>退回修改 KOL（Pending → Rejected），需填退回原因。</summary>
public sealed record RejectKolCommand(long KolId, string RejectionNote);
