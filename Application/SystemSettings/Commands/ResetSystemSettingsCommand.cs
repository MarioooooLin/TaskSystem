namespace Application.SystemSettings.Commands;

/// <summary>將系統參數還原為預設值。</summary>
public sealed record ResetSystemSettingsCommand(string? Note = null);
