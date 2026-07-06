using Domain.Enums;

namespace Domain.Entities;

public class Role
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RoleScope Scope { get; set; }
    public bool IsSystemReserved { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ── Domain 規則（待實作）────────────────────────────
    /// <summary>
    /// 是否可指派給指定 Merchant 成員（Scope 必須為 Merchant 且已啟用）。
    /// </summary>
    public bool CanAssignToMerchantMember()
        => Scope == RoleScope.Merchant && IsActive;
}
