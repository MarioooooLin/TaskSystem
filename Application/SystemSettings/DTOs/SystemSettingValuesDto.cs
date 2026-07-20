namespace Application.SystemSettings.DTOs;

/// <summary>系統參數設定頁所需的核心商務參數值。</summary>
public sealed class SystemSettingValuesDto
{
    public decimal CaseOpeningFeeAmount { get; init; }
    public decimal KolServiceFeeRate { get; init; }
    public decimal AffiliatePlatformCommissionRate { get; init; }
    public decimal AffiliateKolMinCommissionRate { get; init; }
    public decimal CaseAutoExecutionThresholdRate { get; init; }
    public decimal KolMinPayoutAmount { get; init; }
    public decimal KolTaxRate { get; init; }
    public decimal KolPayoutFeeRate { get; init; }
    public decimal KolPayoutFixedFeeAmount { get; init; }
    public string KolPayoutMode { get; init; } = "全額提領";
    public string KolPayoutDays { get; init; } = "10,25";
    public int KolPayoutClosingDayOffset { get; init; } = -5;
    public int CaseReconfirmationDeadlineDays { get; init; } = 3;
}
