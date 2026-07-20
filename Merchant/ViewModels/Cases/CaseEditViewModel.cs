using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Merchant.ViewModels.Cases;

/// <summary>案件新增/編輯頁 ViewModel。</summary>
public sealed class CaseEditViewModel
{
    public long CaseId { get; set; }

    [Required(ErrorMessage = "請輸入案件標題")]
    [MaxLength(200, ErrorMessage = "標題最多 200 字")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "案件說明最多 4000 字")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "請選擇執行縣市")]
    [Display(Name = "城市")]
    public int? CityId { get; set; }

    [Required(ErrorMessage = "請輸入詳細地址")]
    [Display(Name = "地址")]
    [MaxLength(300, ErrorMessage = "地址最多 300 字")]
    public string? Address { get; set; }

    [Display(Name = "官方網站")]
    [MaxLength(500, ErrorMessage = "網址最多 500 字")]
    public string? OfficialUrl { get; set; }

    [Display(Name = "分類")]
    public List<int> Categories { get; set; } = [];

    [Display(Name = "語言")]
    public List<string> Languages { get; set; } = [];

    /// <summary>語言選項（從 Languages 字典表動態載入）。</summary>
    public List<LanguageOptionViewModel> LanguageOptions { get; set; } = [];

    [Display(Name = "發佈平台")]
    public List<short> Platforms { get; set; } = [];

    [Display(Name = "現金報酬")]
    public bool HasCash { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "現金報酬金額需介於 0 至 99,999,999.99")]
    [Display(Name = "單人現金報酬")]
    public decimal? CashRewardAmount { get; set; }

    [Display(Name = "銷售分潤")]
    public bool HasCommission { get; set; }

    [Range(0, 100, ErrorMessage = "分潤比例需介於 0 至 100")]
    [Display(Name = "分潤比例（%）")]
    public decimal? CommissionRate { get; set; }

    [Display(Name = "Cookie 有效天數")]
    [Range(1, 365, ErrorMessage = "Cookie 天數需介於 1 至 365")]
    public int? CookieDays { get; set; }

    [Required(ErrorMessage = "請選擇報名截止日")]
    [Display(Name = "報名截止日")]
    public DateTime? ApplicationDeadline { get; set; }

    [Required(ErrorMessage = "請選擇成果提交截止日")]
    [Display(Name = "成果提交截止日")]
    public DateTime? SubmissionDeadline { get; set; }

    [Required(ErrorMessage = "請輸入預計招募組數")]
    [Range(1, 9999, ErrorMessage = "預計招募組數需介於 1 至 9999")]
    [Display(Name = "預計招募組數")]
    public int WantedKolCount { get; set; } = 5;

    [Required(ErrorMessage = "請輸入交付需求清單")]
    [Display(Name = "交付物說明")]
    [MaxLength(4000, ErrorMessage = "交付物說明最多 4000 字")]
    public string? DeliverableDescription { get; set; }

    [Display(Name = "最低粉絲數")]
    [Range(0, int.MaxValue, ErrorMessage = "最低粉絲數需為非負整數")]
    public int? MinFollowers { get; set; }

    [Display(Name = "應徵條件備註")]
    [MaxLength(2000, ErrorMessage = "備註最多 2000 字")]
    public string? RequirementNotes { get; set; }

    public List<CaseBarterItemEditViewModel> BarterItems { get; set; } = [];

    public List<CaseAttachmentEditViewModel> Attachments { get; set; } = [];

    public CaseStatus Status { get; set; } = CaseStatus.Draft;

    public bool IsNew => CaseId <= 0;

    /// <summary>表單模式：Draft = 儲存草稿，Publish = 前往發布確認。</summary>
    public string SubmitMode { get; set; } = "Draft";
}

public sealed class LanguageOptionViewModel
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class CaseBarterItemEditViewModel
{
    public long? Id { get; set; }

    [Required(ErrorMessage = "請輸入贈品名稱")]
    [MaxLength(200, ErrorMessage = "贈品名稱最多 200 字")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "數量需至少為 1")]
    public int? Quantity { get; set; }

    [MaxLength(500, ErrorMessage = "備註最多 500 字")]
    public string? Note { get; set; }
}

public sealed class CaseAttachmentEditViewModel
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public short AttachmentType { get; set; }
    public DateTime UploadedAt { get; set; }
}
