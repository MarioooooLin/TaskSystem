using Domain.Enums;

namespace Merchant.Models;

/// <summary>案件編輯頁下拉/複選選項對照。</summary>
public static class CaseEditLookups
{
    public static readonly IReadOnlyDictionary<KolCategoryType, string> CategoryNames = new Dictionary<KolCategoryType, string>
    {
        [KolCategoryType.Travel] = "旅遊",
        [KolCategoryType.Food] = "美食",
        [KolCategoryType.Fashion] = "時尚",
        [KolCategoryType.Beauty] = "美妝",
        [KolCategoryType.HomeLiving] = "居家生活",
        [KolCategoryType.Parenting] = "親子",
        [KolCategoryType.Pet] = "寵物",
        [KolCategoryType.Gaming] = "遊戲",
        [KolCategoryType.Music] = "音樂",
        [KolCategoryType.Film] = "影視",
        [KolCategoryType.Art] = "藝術",
        [KolCategoryType.Books] = "書籍",
        [KolCategoryType.Technology] = "科技",
        [KolCategoryType.Finance] = "財經",
        [KolCategoryType.Education] = "教育",
        [KolCategoryType.Career] = "職場",
        [KolCategoryType.Health] = "健康",
        [KolCategoryType.Fitness] = "健身",
        [KolCategoryType.Sports] = "運動",
        [KolCategoryType.Wellness] = "養生",
        [KolCategoryType.Nutrition] = "營養",
        [KolCategoryType.Charity] = "公益",
        [KolCategoryType.Environment] = "環保",
        [KolCategoryType.Politics] = "政治",
        [KolCategoryType.Culture] = "文化",
        [KolCategoryType.Crossover] = "跨界",
    };

    public const string CustomCategoryLabel = "+ 自定義類型";

    public static readonly IReadOnlyDictionary<SocialPlatform, string> PlatformNames = new Dictionary<SocialPlatform, string>
    {
        [SocialPlatform.X] = "X",
        [SocialPlatform.Instagram] = "Instagram",
        [SocialPlatform.Facebook] = "Facebook",
        [SocialPlatform.YouTube] = "YouTube",
        [SocialPlatform.Blog] = "Blog",
        [SocialPlatform.XiaoHongShu] = "小紅書",
        [SocialPlatform.TikTok] = "TikTok",
        [SocialPlatform.Douyin] = "抖音",
        [SocialPlatform.Threads] = "Threads",
        [SocialPlatform.Snapchat] = "Snapchat",
        [SocialPlatform.WeChat] = "WeChat",
    };
}
