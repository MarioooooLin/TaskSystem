namespace Admin.ViewModels.SystemSetting;

/// <summary>系統參數異動紀錄顯示項目。</summary>
public sealed class SystemSettingLogViewModel
{
    public string SettingKey { get; set; } = string.Empty;

    public string SettingName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string NewValue { get; set; } = string.Empty;

    public string? ChangedByName { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Note { get; set; }
}
