using Application.Abstractions.Repositories;

namespace Application.Cases.Commands;

/// <summary>儲存案件草稿（新增或更新）。</summary>
public sealed record SaveCaseDraftCommand(
    long CaseId,
    long MerchantId,
    long CurrentUserId,
    string Title,
    string? Description,
    int? CityId,
    string? Address,
    string? OfficialUrl,
    IReadOnlyList<int> Categories,
    IReadOnlyList<string> Languages,
    IReadOnlyList<short> Platforms,
    bool HasCash,
    decimal? CashRewardAmount,
    bool HasCommission,
    decimal? CommissionRate,
    int? CookieDays,
    DateTime? ApplicationDeadline,
    DateTime? SubmissionDeadline,
    int WantedKolCount,
    string? DeliverableDescription,
    int? MinFollowers,
    string? RequirementNotes,
    IReadOnlyList<CaseBarterItemInput> BarterItems);
