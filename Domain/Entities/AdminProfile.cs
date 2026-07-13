namespace Domain.Entities;

public class AdminProfile
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
