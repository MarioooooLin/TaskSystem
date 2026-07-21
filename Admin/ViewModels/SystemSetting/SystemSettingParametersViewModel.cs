using System.ComponentModel.DataAnnotations;

namespace Admin.ViewModels.SystemSetting;

/// <summary>系統參數設定頁 ViewModel。</summary>
public sealed class SystemSettingParametersViewModel : IValidatableObject
{
    [Display(Name = "案件開案費")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 999999999, ErrorMessage = "{0}必須介於 0 到 999,999,999 之間")]
    public decimal CaseOpeningFeeAmount { get; set; }

    [Display(Name = "平台服務費率")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 100, ErrorMessage = "{0}必須介於 0 到 100 之間")]
    public decimal PlatformServiceFeeRate { get; set; }

    [Display(Name = "平台抽成比例")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 100, ErrorMessage = "{0}必須介於 0 到 100 之間")]
    public decimal AffiliatePlatformCommissionRate { get; set; }

    [Display(Name = "KOL 最低分潤比例")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 100, ErrorMessage = "{0}必須介於 0 到 100 之間")]
    public decimal AffiliateKolMinCommissionRate { get; set; }

    [Display(Name = "案件自動執行門檻")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 100, ErrorMessage = "{0}必須介於 0 到 100 之間")]
    public decimal CaseAutoExecutionThresholdRate { get; set; }

    [Display(Name = "最低提領金額")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 999999999, ErrorMessage = "{0}必須介於 0 到 999,999,999 之間")]
    public decimal KolMinPayoutAmount { get; set; }

    [Display(Name = "KOL 稅金扣除率")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 100, ErrorMessage = "{0}必須介於 0 到 100 之間")]
    public decimal KolTaxRate { get; set; }

    [Display(Name = "KOL 提領手續費率")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 100, ErrorMessage = "{0}必須介於 0 到 100 之間")]
    public decimal KolPayoutFeeRate { get; set; }

    [Display(Name = "KOL 提領固定手續費")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(0, 999999999, ErrorMessage = "{0}必須介於 0 到 999,999,999 之間")]
    public decimal KolPayoutFixedFeeAmount { get; set; }

    [Display(Name = "提領方式")]
    [Required(ErrorMessage = "請輸入{0}")]
    public string KolPayoutMode { get; set; } = "全額提領";

    [Display(Name = "撥款日")]
    [Required(ErrorMessage = "請輸入{0}")]
    [RegularExpression(@"^([1-9]|[12]\d|3[01])(,([1-9]|[12]\d|3[01]))*$", ErrorMessage = "{0}格式不正確，請輸入 1-31 的日期，以逗號分隔")]
    public string KolPayoutDays { get; set; } = "10,25";

    [Display(Name = "關帳日設定")]
    [Required(ErrorMessage = "請輸入{0}")]
    [Range(-30, 0, ErrorMessage = "{0}必須介於 -30 到 0 之間")]
    public int KolPayoutClosingDayOffset { get; set; } = -5;

    /// <summary>異動備註。</summary>
    [Display(Name = "異動備註")]
    [MaxLength(500, ErrorMessage = "{0}不可超過 500 字")]
    public string? Note { get; set; }

    public IReadOnlyList<SystemSettingLogViewModel> RecentLogs { get; set; } = Array.Empty<SystemSettingLogViewModel>();

    public bool CanManage { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AffiliatePlatformCommissionRate + AffiliateKolMinCommissionRate > 100)
        {
            yield return new ValidationResult(
                "平台抽成比例與 KOL 最低分潤比例總和不可超過 100%",
                new[] { nameof(AffiliatePlatformCommissionRate), nameof(AffiliateKolMinCommissionRate) });
        }
    }
}
