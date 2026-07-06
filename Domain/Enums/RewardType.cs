namespace Domain.Enums;

/// <summary>
/// 案件提供的報酬類型（可複合）。
/// Cash：現金獎勵；Commission：銷售佣金；Barter：實物體驗。
/// </summary>
public enum RewardType : short
{
    Cash = 1,
    Commission = 2,
    Barter = 3
}
