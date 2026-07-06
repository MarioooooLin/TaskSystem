using Application.Abstractions.Security;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Authentication;

/// <summary>
/// BCrypt 密碼雜湊實作。
/// WorkFactor 12 在現代硬體約耗時 250ms，平衡安全性與效能。
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BC.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BC.Verify(password, hash);
}
