using Domain.Enums;

namespace Merchant.ViewModels.Cases;

/// <summary>業者端案件管理列表篩選條件 ViewModel。</summary>
public sealed class CaseListQueryViewModel
{
    public string? Keyword { get; set; }

    /// <summary>案件狀態篩選（null=全部）。</summary>
    public CaseStatus? Status { get; set; }

    /// <summary>已結案篩選（true=只顯示已結案/已取消）。</summary>
    public bool? ClosedOnly { get; set; }

    /// <summary>
    /// 合作條件篩選：null=全部，1=現金報酬，2=贈品項目，3=現金報酬+贈品項目。
    /// </summary>
    public int? RewardTypeFilter { get; set; }

    /// <summary>發佈平台篩選（null=全部）。</summary>
    public SocialPlatform? Platform { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
