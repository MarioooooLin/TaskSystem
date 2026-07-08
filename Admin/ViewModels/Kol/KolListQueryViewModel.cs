using Domain.Enums;

namespace Admin.ViewModels.Kol;

/// <summary>KOL 列表頁搜尋條件 ViewModel（ADM-005）。</summary>
public sealed class KolListQueryViewModel
{
    public string? Keyword { get; set; }
    public VerificationStatus? VerificationStatus { get; set; }

    /// <summary>KOL 類型（可多選）。</summary>
    public List<short> Categories { get; set; } = [];

    /// <summary>主要平台（可多選）。</summary>
    public List<short> Platforms { get; set; } = [];

    /// <summary>是否已填收款資料。</summary>
    public bool? HasBankAccount { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
