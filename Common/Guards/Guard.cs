using Common.Errors;

namespace Common.Guards;

/// <summary>
/// 輸入防衛工具。驗證失敗時拋出 ArgumentException。
/// 用於 Application 層的 Use Case 入口參數驗證（非業務規則）。
/// </summary>
public static class Guard
{
    public static void AgainstNull<T>(T? value, string paramName)
        where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
    }

    public static void AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("值不可為 null 或空白。", paramName);
    }

    public static void AgainstNegative(decimal value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, "值不可為負數。");
    }

    public static void AgainstZeroOrNegative(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, "值必須大於零。");
    }

    public static void AgainstZeroOrNegative(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, "值必須大於零。");
    }

    public static void AgainstPastDate(DateTime value, string paramName, DateTime? referenceTime = null)
    {
        var now = referenceTime ?? DateTime.UtcNow;
        if (value <= now)
            throw new ArgumentOutOfRangeException(paramName, "日期必須為未來時間。");
    }

    public static void AgainstInvalidRange(decimal value, decimal min, decimal max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, $"值必須介於 {min} 與 {max} 之間。");
    }
}
