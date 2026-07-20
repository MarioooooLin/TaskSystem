namespace Application.Cases.Queries;

/// <summary>取得案件發布預覽與預算試算。</summary>
public sealed record GetPublishPreviewQuery(
    long CaseId,
    long MerchantId);
