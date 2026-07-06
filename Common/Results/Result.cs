using Common.Errors;

namespace Common.Results;

/// <summary>
/// 無回傳值的操作結果，代表成功或失敗（含 Error）。
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("成功結果不能包含錯誤。");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("失敗結果必須包含錯誤。");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // ── 工廠方法 ─────────────────────────────────────────
    public static Result Success()
        => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value)
        => new(value, true, Error.None);

    public static Result Failure(Error error)
        => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error)
        => new(default, false, error);
}
