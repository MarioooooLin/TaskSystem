namespace Application.Cases.Queries;

/// <summary>載入案件編輯頁資料。</summary>
public sealed record GetCaseEditQuery(
    long CaseId,
    long MerchantId);
