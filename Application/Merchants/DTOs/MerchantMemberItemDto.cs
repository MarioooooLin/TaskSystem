using Domain.Enums;

namespace Application.Merchants.DTOs;

/// <summary>業者成員列表列項（詳情頁成員區塊）。</summary>
public sealed class MerchantMemberItemDto
{
    public long MemberId { get; init; }
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public MerchantMemberStatus Status { get; init; }
    public DateTime JoinedAt { get; init; }
}
