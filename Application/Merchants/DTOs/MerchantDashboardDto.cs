namespace Application.Merchants.DTOs;

/// <summary>業者端首頁儀表板資料。</summary>
public sealed class MerchantDashboardDto
{
    /// <summary>業者公司名稱。</summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>業者狀態：啟用中 / 停用中。</summary>
    public string StatusLabel { get; init; } = string.Empty;

    /// <summary>錢包資料。</summary>
    public MerchantDashboardWalletDto Wallet { get; init; } = new();

    /// <summary>各案件狀態統計。</summary>
    public IReadOnlyList<MerchantDashboardStatusCountDto> StatusCounts { get; init; } = [];

    /// <summary>待辦事項列表。</summary>
    public IReadOnlyList<MerchantDashboardTodoDto> Todos { get; init; } = [];

    /// <summary>最新案件列表。</summary>
    public IReadOnlyList<MerchantDashboardRecentCaseDto> RecentCases { get; init; } = [];

    /// <summary>待驗收通知數量（給右下角 toast 用）。</summary>
    public int PendingAcceptanceCount { get; init; }
}

public sealed class MerchantDashboardWalletDto
{
    public decimal AvailableAmount { get; init; }
    public decimal FrozenAmount { get; init; }
    public decimal TotalAmount { get; init; }
}

public sealed class MerchantDashboardStatusCountDto
{
    public CaseStatusCategory Category { get; init; }
    public string Label { get; init; } = string.Empty;
    public string IconUrl { get; init; } = string.Empty;
    public int Count { get; init; }
}

public enum CaseStatusCategory
{
    Draft = 1,
    Recruiting = 2,
    InProgress = 3,
    PendingAcceptance = 4,
    Closed = 5
}

public sealed class MerchantDashboardTodoDto
{
    public long CaseId { get; init; }
    public string TodoType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusCssClass { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string ActionText { get; init; } = string.Empty;
    public string ActionUrl { get; init; } = string.Empty;
}

public sealed class MerchantDashboardRecentCaseDto
{
    public long CaseId { get; init; }
    public string TypeLabel { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusCssClass { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string ActionText { get; init; } = string.Empty;
    public string ActionUrl { get; init; } = string.Empty;
}
