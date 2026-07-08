namespace Admin.ViewModels.Kol;

/// <summary>審核新進 KOL 列表搜尋條件 ViewModel（ADM-015）。</summary>
public sealed class KolReviewListQueryViewModel
{
    public string? Keyword { get; set; }

    /// <summary>狀態 chip：pending / resubmit / returned / approved。null = 全部待審（Pending）。</summary>
    public string? StatusFilter { get; set; }

    public short? Category { get; set; }
    public short? Platform { get; set; }

    /// <summary>送審日期（yyyy-MM-dd），對應 KolProfiles.UpdatedAt 日期。</summary>
    public string? SubmittedDate { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
