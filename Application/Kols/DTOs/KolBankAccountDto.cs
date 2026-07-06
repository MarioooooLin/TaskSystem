namespace Application.Kols.DTOs;

/// <summary>收款資料 DTO（Admin 查看用，帳號中間碼已遮蔽）。</summary>
public sealed class KolBankAccountDto
{
    /// <summary>1=個人 2=公司</summary>
    public short AccountType { get; init; }

    public string AccountName { get; init; } = string.Empty;
    public string BankCode { get; init; } = string.Empty;
    public string? BankName { get; init; }

    /// <summary>帳號已遮蔽中間碼，例如 8220-****-4519。</summary>
    public string MaskedAccountNumber { get; init; } = string.Empty;

    /// <summary>1=Pending 2=Verified 3=Rejected</summary>
    public short Status { get; init; }

    public DateTime UpdatedAt { get; init; }
}
