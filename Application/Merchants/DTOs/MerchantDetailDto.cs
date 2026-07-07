using Domain.Enums;

namespace Application.Merchants.DTOs;

/// <summary>業者詳情頁完整聚合 DTO。</summary>
public sealed class MerchantDetailDto
{
    // ── 基本資料 ──────────────────────────────────────────
    public long MerchantId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string? EnglishName { get; init; }
    public string? TaxId { get; init; }
    public string? IndustryType { get; init; }
    public string? ContactName { get; init; }
    public string? Phone { get; init; }
    public string? Fax { get; init; }
    public string? CompanyEmail { get; init; }
    public string? Website { get; init; }
    public string? Address { get; init; }
    public DateOnly? EstablishedDate { get; init; }

    /// <summary>Owner 帳號 Email（來自 Users）。</summary>
    public string OwnerEmail { get; init; } = string.Empty;

    public VerificationStatus VerificationStatus { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public string? UpdatedByAdminName { get; init; }
    public DateTime CreatedAt { get; init; }

    // ── 聯絡窗口 ──────────────────────────────────────────
    public IReadOnlyList<MerchantContactDto> Contacts { get; init; } = [];

    // ── 統計數字 ──────────────────────────────────────────
    public MerchantStatsDto Stats { get; init; } = new();

    // ── 近期案件（最新 10 筆） ────────────────────────────
    public IReadOnlyList<MerchantCaseSummaryDto> RecentCases { get; init; } = [];

    // ── 錢包概況 ──────────────────────────────────────────
    public MerchantWalletSummaryDto Wallet { get; init; } = new();

    // ── 近期錢包交易（最新 10 筆） ───────────────────────
    public IReadOnlyList<MerchantWalletTransactionDto> RecentTransactions { get; init; } = [];

    // ── 折扣金錢包概況 ────────────────────────────────────
    public MerchantCreditWalletSummaryDto CreditWallet { get; init; } = new();

    // ── 近期折扣金加值/扣回（最新 5 筆） ─────────────────
    public IReadOnlyList<MerchantCreditTransactionDto> RecentCreditGrants { get; init; } = [];

    // ── 近期折扣金使用（最新 5 筆） ──────────────────────
    public IReadOnlyList<MerchantCreditTransactionDto> RecentCreditUsages { get; init; } = [];

    // ── 成員列表 ──────────────────────────────────────────
    public IReadOnlyList<MerchantMemberItemDto> Members { get; init; } = [];

    // ── 近期活動紀錄（最新 10 筆） ───────────────────────
    public IReadOnlyList<MerchantActivityLogDto> RecentActivityLogs { get; init; } = [];
}
