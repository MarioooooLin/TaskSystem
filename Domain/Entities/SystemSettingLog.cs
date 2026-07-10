namespace Domain.Entities;

/// <summary>系統參數異動紀錄。</summary>
public class SystemSettingLog
{
    public long Id { get; set; }

    /// <summary>異動的參數 Key。</summary>
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>原設定值。</summary>
    public string? OldValue { get; set; }

    /// <summary>新設定值。</summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>異動備註。</summary>
    public string? Note { get; set; }

    public long? ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; }
}
