using System.Text.Json;
using Application.SystemSettings.DTOs;

namespace Application.Cases.Budget;

/// <summary>案件發布前預算試算器。</summary>
public sealed class CaseBudgetCalculator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SystemSettingValuesDto _settings;

    public CaseBudgetCalculator(SystemSettingValuesDto settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// 根據單人報酬、招募組數與可用折扣金計算預算明細。
    /// 平台服務費 = (報酬小計 + 開案費 - 折扣) × 平台服務費率。
    /// </summary>
    public CaseBudgetBreakdown Calculate(
        decimal rewardAmountPerKol,
        int wantedKolCount,
        decimal? availableCredit = null)
    {
        var rewardSubtotal = rewardAmountPerKol * wantedKolCount;
        var openingFee = _settings.CaseOpeningFeeAmount;

        // 折扣金僅可折抵開案費
        var credit = availableCredit ?? 0m;
        var discountAmount = credit >= openingFee ? openingFee : credit;

        // 平台服務費計算基礎 = 報酬小計 + 開案費 - 折扣
        var platformServiceFeeBase = rewardSubtotal + openingFee - discountAmount;
        var platformServiceFee = Round(platformServiceFeeBase * _settings.PlatformServiceFeeRate / 100m);

        var feeItems = new List<CaseFeeItem>
        {
            new() { Name = "案件開案費", Amount = openingFee },
            new() { Name = "折扣", Amount = -discountAmount },
            new() { Name = "平台服務費", Amount = platformServiceFee, Note = $"費率 {_settings.PlatformServiceFeeRate}%" }
        };

        var estimatedFrozenAmount = rewardSubtotal
            + openingFee
            - discountAmount
            + platformServiceFee;

        return new CaseBudgetBreakdown
        {
            RewardAmountPerKol = rewardAmountPerKol,
            WantedKolCount = wantedKolCount,
            RewardSubtotal = rewardSubtotal,
            CaseOpeningFee = openingFee,
            DiscountAmount = discountAmount,
            PlatformServiceFee = platformServiceFee,
            EstimatedFrozenAmount = estimatedFrozenAmount,
            FeeItemsJson = JsonSerializer.Serialize(feeItems, JsonOptions)
        };
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
