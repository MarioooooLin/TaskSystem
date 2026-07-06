namespace Application.Abstractions.Security;

/// <summary>
/// 密碼雜湊服務介面，由 Infrastructure 實作。
/// Application 層只依賴介面，不感知演算法細節。
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 將明文密碼雜湊化。
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// 驗證明文密碼是否符合儲存的雜湊值。
    /// </summary>
    bool Verify(string password, string hash);
}
