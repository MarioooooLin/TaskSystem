using Application.Abstractions.Persistence;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>
/// Case Repository 介面。
/// 負責取得與儲存需要變更的 Case Entity，參與 Transaction。
/// </summary>
public interface ICaseRepository
{
    Task<Case?> GetByIdAsync(long id, IDbSession session, CancellationToken ct = default);

    /// <summary>取得 Case，確認同時屬於指定 Merchant（Ownership 防護）。</summary>
    Task<Case?> GetByIdAndMerchantAsync(long id, long merchantId, IDbSession session, CancellationToken ct = default);

    Task<long> InsertAsync(Case caseEntity, IDbSession session, CancellationToken ct = default);

    Task UpdateAsync(Case caseEntity, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件完整子表資料（編輯頁使用）。</summary>
    Task<CaseEditData?> GetEditDataAsync(long caseId, long merchantId, IDbSession session, CancellationToken ct = default);

    /// <summary>取得案件最新一筆預算快照。</summary>
    Task<CaseBudgetSnapshot?> GetLatestBudgetSnapshotAsync(long caseId, IDbSession session, CancellationToken ct = default);

    /// <summary>依 IdempotencyKey 查詢預算快照，用於防止重複發布。</summary>
    Task<bool> ExistsBudgetSnapshotByIdempotencyKeyAsync(string idempotencyKey, IDbSession session, CancellationToken ct = default);

    /// <summary>同步案件子表資料（分類、語言、平台、以物易物、需求）。</summary>
    Task SyncSubtablesAsync(
        long caseId,
        IReadOnlyList<int> categories,
        IReadOnlyList<string> languages,
        IReadOnlyList<short> platforms,
        IReadOnlyList<CaseBarterItemInput> barterItems,
        int? minFollowers,
        string? requirementNotes,
        IDbSession session,
        CancellationToken ct = default);
}

public sealed class CaseBarterItemInput
{
    public long? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int? Quantity { get; init; }
    public string? Note { get; init; }
}

/// <summary>
/// 案件編輯所需完整資料（含子表）。
/// </summary>
public sealed class CaseEditData
{
    public Case Case { get; set; } = null!;
    public List<int> Categories { get; set; } = [];
    public List<string> Languages { get; set; } = [];
    public List<short> Platforms { get; set; } = [];
    public List<CaseBarterItem> BarterItems { get; set; } = [];
    public CaseRequirements? Requirements { get; set; }
    public List<CaseAttachment> Attachments { get; set; } = [];
}

public sealed class CaseRequirements
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public int? MinFollowers { get; set; }
    public string? Notes { get; set; }
}

public sealed class CaseBarterItem
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? Quantity { get; set; }
    public string? Note { get; set; }
}
