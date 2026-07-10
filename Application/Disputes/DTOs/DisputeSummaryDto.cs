namespace Application.Disputes.DTOs;

public sealed class DisputeSummaryDto
{
    public int TotalCount { get; set; }
    public int OpenCount { get; set; }
    public int UnderReviewCount { get; set; }
    public int RevisionCount { get; set; }
    public int ResolvedCount { get; set; }
    public int TodayNewCount { get; set; }
}
