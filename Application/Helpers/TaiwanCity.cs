namespace Application.Helpers;

/// <summary>台灣縣市對照輔助類別。</summary>
public static class TaiwanCity
{
    /// <summary>ID 對應縣市名稱。</summary>
    public static readonly IReadOnlyDictionary<int, string> Names = new Dictionary<int, string>
    {
        [1] = "台北市",
        [2] = "新北市",
        [3] = "桃園市",
        [4] = "台中市",
        [5] = "台南市",
        [6] = "高雄市",
        [7] = "基隆市",
        [8] = "新竹市",
        [9] = "新竹縣",
        [10] = "苗栗縣",
        [11] = "彰化縣",
        [12] = "南投縣",
        [13] = "雲林縣",
        [14] = "嘉義市",
        [15] = "嘉義縣",
        [16] = "屏東縣",
        [17] = "宜蘭縣",
        [18] = "花蓮縣",
        [19] = "台東縣",
        [20] = "澎湖縣",
        [21] = "金門縣",
        [22] = "連江縣",
    };

    /// <summary>依 ID 取得縣市名稱；找不到時回傳 null。</summary>
    public static string? GetName(int? cityId)
    {
        if (cityId is null) return null;

        return Names.TryGetValue(cityId.Value, out var name) ? name : null;
    }

    /// <summary>依 ID 字串取得縣市名稱；找不到時回傳 null。</summary>
    public static string? GetName(string? cityId)
    {
        if (string.IsNullOrWhiteSpace(cityId)) return null;
        if (int.TryParse(cityId, out var id)) return GetName(id);

        return null;
    }
}
