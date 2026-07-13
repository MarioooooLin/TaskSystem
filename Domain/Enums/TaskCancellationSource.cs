namespace Domain.Enums;

/// <summary>
/// 任務取消來源。
/// </summary>
public enum TaskCancellationSource : short
{
    /// <summary>未指定（預設 / 歷史資料）。</summary>
    Unspecified = 0,

    /// <summary>KOL 放棄任務。</summary>
    KolAbandoned = 1,

    /// <summary>業者取消案件 / 取消任務。</summary>
    MerchantCancelled = 2,

    /// <summary>系統或管理員取消。</summary>
    SystemCancelled = 3,
}
