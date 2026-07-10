namespace Domain.Entities;

/// <summary>平台系統參數。</summary>
public class SystemSetting
{
    public long Id { get; set; }

    /// <summary>參數 Key，全站唯一。</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>目前值（字串儲存，依 ValueType 解析）。</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>還原預設時使用的預設值。</summary>
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>值類型：string / number / percent / boolean / json。</summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>參數分組。</summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>參數說明。</summary>
    public string? Description { get; set; }

    public long? UpdatedByUserId { get; set; }

    public DateTime UpdatedAt { get; set; }
}
