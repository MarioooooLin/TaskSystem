namespace Domain.Entities;

/// <summary>系統支援語言字典。</summary>
public sealed class Language
{
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;
}
