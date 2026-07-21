namespace Application.SystemSettings.Commands;

/// <summary>更新平台系統參數。</summary>
public sealed record UpdateSystemSettingsCommand(
    decimal CaseOpeningFeeAmount,
    decimal PlatformServiceFeeRate,
    decimal AffiliatePlatformCommissionRate,
    decimal AffiliateKolMinCommissionRate,
    decimal CaseAutoExecutionThresholdRate,
    decimal KolMinPayoutAmount,
    decimal KolTaxRate,
    decimal KolPayoutFeeRate,
    decimal KolPayoutFixedFeeAmount,
    string KolPayoutMode,
    string KolPayoutDays,
    int KolPayoutClosingDayOffset,
    string? Note = null);
