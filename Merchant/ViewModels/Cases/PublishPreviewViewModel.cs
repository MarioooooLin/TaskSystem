using System.ComponentModel.DataAnnotations;

namespace Merchant.ViewModels.Cases;

/// <summary>案件發布確認頁 ViewModel。</summary>
public sealed class PublishPreviewViewModel
{
    public long CaseId { get; set; }
    public string Title { get; set; } = string.Empty;

    public string? City { get; set; }
    public string? Address { get; set; }

    public int WantedKolCount { get; set; }
    public decimal RewardAmountPerKol { get; set; }
    public decimal RewardSubtotal { get; set; }

    public bool HasCommission { get; set; }
    public decimal? CommissionRate { get; set; }
    public List<PublishPreviewBarterItemViewModel> BarterItems { get; set; } = [];

    public List<short> Platforms { get; set; } = [];
    public string? DeliverableDescription { get; set; }

    public List<PublishPreviewAttachmentViewModel> Attachments { get; set; } = [];

    public List<PublishPreviewFeeItemViewModel> FeeItems { get; set; } = [];
    public decimal CaseOpeningFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PlatformServiceFee { get; set; }
    public decimal EstimatedFrozenAmount { get; set; }

    public decimal CurrentWalletBalance { get; set; }
    public bool HasEnoughBalance { get; set; }

    public DateTime ApplicationDeadline { get; set; }
    public DateTime SubmissionDeadline { get; set; }

    public string IdempotencyKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "請確認發布條款")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "請確認發布條款")]
    public bool Confirmed { get; set; }
}

public sealed class PublishPreviewFeeItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

public sealed class PublishPreviewBarterItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public string? Note { get; set; }
}

public sealed class PublishPreviewAttachmentViewModel
{
    public long Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
}
