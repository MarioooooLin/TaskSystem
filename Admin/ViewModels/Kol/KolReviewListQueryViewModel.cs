using Domain.Enums;

namespace Admin.ViewModels.Kol;

/// <summary>審核新進 KOL 列表搜尋條件 ViewModel（ADM-015）。</summary>
public sealed class KolReviewListQueryViewModel
{
    public string? Keyword { get; set; }
    public VerificationStatus? VerificationStatus { get; set; }
    public short? Category { get; set; }
    public short? Platform { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
