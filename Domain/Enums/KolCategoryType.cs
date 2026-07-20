namespace Domain.Enums;

/// <summary>
/// KOL 類型分類，對應 KolCategories.Category 與 CaseCategories.Category。
/// 集中定義避免 View 與各層硬編碼，值域 1~26。
/// </summary>
public enum KolCategoryType : short
{
    Travel = 1,        // 旅遊
    Food = 2,          // 美食
    Fashion = 3,       // 時尚
    Beauty = 4,        // 美妝
    HomeLiving = 5,    // 居家生活
    Parenting = 6,     // 親子
    Pet = 7,           // 寵物
    Gaming = 8,        // 遊戲
    Music = 9,         // 音樂
    Film = 10,         // 影視
    Art = 11,          // 藝術
    Books = 12,        // 書籍
    Technology = 13,   // 科技
    Finance = 14,      // 財經
    Education = 15,    // 教育
    Career = 16,       // 職場
    Health = 17,       // 健康
    Fitness = 18,      // 健身
    Sports = 19,       // 運動
    Wellness = 20,     // 養生
    Nutrition = 21,    // 營養
    Charity = 22,      // 公益
    Environment = 23,  // 環保
    Politics = 24,     // 政治
    Culture = 25,      // 文化
    Crossover = 26     // 跨界
}
