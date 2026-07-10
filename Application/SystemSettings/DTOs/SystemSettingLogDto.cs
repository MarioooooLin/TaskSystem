namespace Application.SystemSettings.DTOs;

/// <summary>系統參數異動紀錄。</summary>
public sealed class SystemSettingLogDto
{
    public string SettingKey { get; init; } = string.Empty;

    /// <summary>參數顯示名稱。</summary>
    public string SettingName { get; init; } = string.Empty;

    public string? OldValue { get; init; }

    public string NewValue { get; init; } = string.Empty;

    /// <summary>操作者名稱（後台帳號名稱）。</summary>
    public string? ChangedByName { get; init; }

    public DateTime ChangedAt { get; init; }

    public string? Note { get; init; }
}
