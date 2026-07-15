namespace Application.Disputes.DTOs;

public sealed class DisputeContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LineStatus { get; set; }
}
