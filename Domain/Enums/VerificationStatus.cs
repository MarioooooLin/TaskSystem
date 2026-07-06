namespace Domain.Enums;

/// <summary>
/// 用於 Merchants 與 KolProfiles 的審核狀態。
/// </summary>
public enum VerificationStatus : short
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Suspended = 4
}
