using Domain.Enums;

namespace Domain.Entities;

public class Merchant
{
    public long Id { get; set; }
    public long UserId { get; set; }  // 初始建立者 (Owner)

    public string CompanyName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? TaxId { get; set; }
    public string? IndustryType { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? CompanyEmail { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public DateOnly? EstablishedDate { get; set; }

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Approved;
    public DateTime? VerifiedAt { get; set; }
    public long? UpdatedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
