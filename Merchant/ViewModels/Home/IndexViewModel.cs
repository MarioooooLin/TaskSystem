namespace Merchant.ViewModels.Home;

public sealed class IndexViewModel
{
    public string CompanyName { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;

    public WalletViewModel Wallet { get; set; } = new();

    public IReadOnlyList<StatusCountViewModel> StatusCounts { get; set; } = [];
    public IReadOnlyList<TodoViewModel> Todos { get; set; } = [];
    public IReadOnlyList<RecentCaseViewModel> RecentCases { get; set; } = [];

    public int PendingReviewCount { get; set; }

    public sealed class WalletViewModel
    {
        public decimal AvailableAmount { get; set; }
        public decimal FrozenAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public sealed class StatusCountViewModel
    {
        public int Category { get; set; }
        public string Label { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class TodoViewModel
    {
        public long CaseId { get; set; }
        public string TodoType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
    }

    public sealed class RecentCaseViewModel
    {
        public long CaseId { get; set; }
        public string TypeLabel { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
    }
}
