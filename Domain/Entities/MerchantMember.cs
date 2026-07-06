using Domain.Enums;

namespace Domain.Entities;

public class MerchantMember
{
    public long Id { get; set; }
    public long MerchantId { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }  // 第一版每人一個 Merchant Scope 角色

    public MerchantMemberStatus Status { get; set; } = MerchantMemberStatus.Active;
    public DateTime JoinedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Domain 規則（待實作）────────────────────────────
    public bool IsActive() => Status == MerchantMemberStatus.Active;
}
