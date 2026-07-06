namespace Application.Merchants.DTOs;

public sealed class MerchantContactDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Title { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
