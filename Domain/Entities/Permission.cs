namespace Domain.Entities;

public class Permission
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;  // e.g. Merchant.Case.Create
    public string? Description { get; set; }

    /// <summary>1=Normal 2=HighRisk</summary>
    public short RiskLevel { get; set; } = 1;
}
