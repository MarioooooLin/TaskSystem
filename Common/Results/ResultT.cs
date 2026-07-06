using Common.Errors;

namespace Common.Results;

/// <summary>
/// 帶回傳值的操作結果。
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// 取得成功時的回傳值。失敗時存取會拋出例外。
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("失敗結果沒有回傳值，請先檢查 IsSuccess。");

    // ── 隱式轉換：讓呼叫端可直接回傳 TValue ──────────────
    public static implicit operator Result<TValue>(TValue value)
        => Result.Success(value);

    public static implicit operator Result<TValue>(Error error)
        => Result.Failure<TValue>(error);
}
