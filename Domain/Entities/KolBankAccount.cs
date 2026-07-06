namespace Domain.Entities;

public class KolBankAccount
{
    public long Id { get; set; }
    public long KolId { get; set; }

    /// <summary>1=個人 2=公司</summary>
    public short AccountType { get; set; } = 1;

    public string AccountName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>1=Pending 2=Verified 3=Rejected</summary>
    public short Status { get; set; } = 1;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
