namespace Common.Errors;

/// <summary>
/// 代表一個預期內的業務錯誤。
/// 用於 Application Use Case 與 Domain 規則的失敗回傳，不代表例外。
/// </summary>
public sealed class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error Unexpected = new("General.Unexpected", "An unexpected error occurred.", ErrorType.Unexpected);

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    // ── 工廠方法：依錯誤類型建立 ──────────────────────────
    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Validation(string code, string description)
        => new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    public static Error Forbidden(string code, string description)
        => new(code, description, ErrorType.Forbidden);

    public static Error Problem(string code, string description)
        => new(code, description, ErrorType.Problem);

    // ── 相等比較 ──────────────────────────────────────────
    public bool Equals(Error? other)
        => other is not null && Code == other.Code && Type == other.Type;

    public override bool Equals(object? obj)
        => obj is Error error && Equals(error);

    public override int GetHashCode()
        => HashCode.Combine(Code, Type);

    public static bool operator ==(Error left, Error right) => left.Equals(right);
    public static bool operator !=(Error left, Error right) => !left.Equals(right);

    public override string ToString() => $"[{Type}] {Code}: {Description}";
}
