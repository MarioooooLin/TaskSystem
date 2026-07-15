namespace Application.Disputes.DTOs;

public sealed class DisputeTimelineDto
{
    public DateTime CreatedAt { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
