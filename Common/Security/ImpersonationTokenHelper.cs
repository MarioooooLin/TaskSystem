using System.Security.Cryptography;
using System.Text;

namespace Common.Security;

/// <summary>
/// 產生與雜湊代理登入一次性 token 的輔助類別。
/// 明文 token 只會在建立時出現一次，之後皆以 SHA-256 hash 儲存與比對。
/// </summary>
public static class ImpersonationTokenHelper
{
    /// <summary>產生 256-bit entropy 的明文 token（64 個十六進位字元）。</summary>
    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>計算 token 的 SHA-256 hash（64 個十六進位小寫字元）。</summary>
    public static string ComputeHash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
