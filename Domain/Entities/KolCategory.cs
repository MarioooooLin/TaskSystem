namespace Domain.Entities;

public class KolCategory
{
    public long Id { get; set; }
    public long KolId { get; set; }

    /// <summary>
    /// 類型代碼（對應 KolCategories.Category）：
    /// 1=旅遊 2=旅行 3=時尚 4=美妝 5=居家生活 6=親子 7=寵物
    /// 8=遊戲 9=音樂 10=影視 11=藝術 12=書籍 13=科技 14=財經
    /// 15=教育 16=職場 17=健康 18=健身 19=運動 20=飲食
    /// 21=養生 22=公益 23=環保 24=政治 25=文化 26=跨界
    /// </summary>
    public short Category { get; set; }
}
