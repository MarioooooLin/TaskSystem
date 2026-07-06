using Domain.Enums;

namespace Application.Kols.DTOs;

/// <summary>
/// KOL 詳情頁基本資料 DTO（由 IKolRepository.GetDetailBaseAsync 回傳）。
/// 子集合（社群帳號、收款資料、任務、收益、活動）由各自 Repository 另外查詢。
/// </summary>
public sealed class KolDetailBaseDto
{
    public long KolId { get; init; }
    public long UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? RealName { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? LineContactId { get; init; }
    public string? Intro { get; init; }
    public bool AcceptsCash { get; init; }
    public bool AcceptsBarter { get; init; }
    public bool AcceptsCommission { get; init; }
    public int? FollowersCount { get; init; }
    public VerificationStatus VerificationStatus { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public string? VerifiedByAdminName { get; init; }
    public string? RejectionNote { get; init; }
    public string? SuspensionNote { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
