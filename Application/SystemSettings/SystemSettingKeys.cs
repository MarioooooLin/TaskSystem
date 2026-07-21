namespace Application.SystemSettings;

/// <summary>系統參數 Key 常數與顯示名稱對照。</summary>
public static class SystemSettingKeys
{
    public const string CaseOpeningFeeAmount = "case_opening_fee_amount";
    public const string PlatformServiceFeeRate = "platform_service_fee_rate";
    public const string AffiliatePlatformCommissionRate = "affiliate_platform_commission_rate";
    public const string AffiliateKolMinCommissionRate = "affiliate_kol_min_commission_rate";
    public const string CaseAutoExecutionThresholdRate = "case_auto_execution_threshold_rate";
    public const string KolMinPayoutAmount = "kol_min_payout_amount";
    public const string KolTaxRate = "kol_tax_rate";
    public const string KolPayoutFeeRate = "kol_payout_fee_rate";
    public const string KolPayoutFixedFeeAmount = "kol_payout_fixed_fee_amount";
    public const string KolPayoutMode = "kol_payout_mode";
    public const string KolPayoutDays = "kol_payout_days";
    public const string KolPayoutClosingDayOffset = "kol_payout_closing_day_offset";

    /// <summary>招募中案件修改後，已錄取 KOL 重新確認期限（日曆天）。</summary>
    public const string CaseReconfirmationDeadlineDays = "case_reconfirmation_deadline_days";

    /// <summary>還原預設值的彙總異動紀錄 Key。</summary>
    public const string ResetAll = "reset_all";

    /// <summary>系統參數定義後設資料。</summary>
    public sealed record Definition(string ValueType, string Group, string? Description, string DefaultValue);

    /// <summary>Key 對應的定義後設資料；用於自動建立遺失的參數。</summary>
    public static readonly IReadOnlyDictionary<string, Definition> Definitions = new Dictionary<string, Definition>(StringComparer.OrdinalIgnoreCase)
    {
        [CaseOpeningFeeAmount] = new("number", "case_fee", "案件固定開案費；案件發布預估凍結金額使用", "1000"),
        [PlatformServiceFeeRate] = new("percent", "case_fee", "平台服務費率；案件發布預估凍結金額使用", "0"),
        [AffiliatePlatformCommissionRate] = new("percent", "commission", "導購平台抽成比例；平台固定保留此比例", "0"),
        [AffiliateKolMinCommissionRate] = new("percent", "commission", "KOL 最低分潤比例；與平台抽成比例合計為業者佣金最低比例", "0"),
        [CaseAutoExecutionThresholdRate] = new("percent", "case", "案件自動執行門檻；招募截止時錄取人數需達預計招募人數比例", "50"),
        [KolMinPayoutAmount] = new("number", "payout", "KOL 最低提領門檻；金額需 >= 此值才可提領", "1000"),
        [KolTaxRate] = new("percent", "payout", "KOL 稅金扣除率", "0"),
        [KolPayoutFeeRate] = new("percent", "payout", "KOL 提領手續費率", "0"),
        [KolPayoutFixedFeeAmount] = new("number", "payout", "KOL 提領固定手續費", "0"),
        [KolPayoutMode] = new("string", "payout", "提領方式", "全額提領"),
        [KolPayoutDays] = new("string", "payout", "撥款日", "10,25"),
        [KolPayoutClosingDayOffset] = new("number", "payout", "關帳日設定", "-5"),
        [CaseReconfirmationDeadlineDays] = new("number", "case", "招募中案件修改後，已錄取 KOL 重新確認期限（日曆天）", "3"),
    };

    /// <summary>Key 對應的中文顯示名稱。</summary>
    public static readonly IReadOnlyDictionary<string, string> Names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [CaseOpeningFeeAmount] = "案件開案費",
        [PlatformServiceFeeRate] = "平台服務費率",
        [AffiliatePlatformCommissionRate] = "平台抽成比例",
        [AffiliateKolMinCommissionRate] = "KOL 最低分潤比例",
        [CaseAutoExecutionThresholdRate] = "案件自動執行門檻",
        [KolMinPayoutAmount] = "最低提領金額",
        [KolTaxRate] = "KOL 稅金扣除率",
        [KolPayoutFeeRate] = "KOL 提領手續費率",
        [KolPayoutFixedFeeAmount] = "KOL 提領固定手續費",
        [KolPayoutMode] = "提領方式",
        [KolPayoutDays] = "撥款日",
        [KolPayoutClosingDayOffset] = "關帳日設定",
        [CaseReconfirmationDeadlineDays] = "重新確認期限天數",
        [ResetAll] = "還原預設",
    };
}
