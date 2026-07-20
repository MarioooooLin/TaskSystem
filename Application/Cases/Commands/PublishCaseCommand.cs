namespace Application.Cases.Commands;

/// <summary>發布案件（正式上線招募）。</summary>
public sealed record PublishCaseCommand(
    long CaseId,
    long MerchantId,
    long CurrentUserId,
    string IdempotencyKey);
