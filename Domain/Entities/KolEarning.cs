using Domain.Enums;

namespace Domain.Entities;

public class KolEarning
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public long TaskId { get; set; }
    public long KolId { get; set; }

    public KolEarningSourceType SourceType { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal? PlatformFeeAmount { get; set; }  // 公式待定
    public decimal NetAmount { get; set; }

    public KolEarningStatus Status { get; set; } = KolEarningStatus.Pending;
    public DateTime? AvailableAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
