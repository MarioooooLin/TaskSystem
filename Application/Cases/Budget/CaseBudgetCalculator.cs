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
    /// 根據單人報酬與招募組數計算預算明細。
    /// </summary>
    public CaseBudgetBreakdown Calculate(
        decimal rewardAmountPerKol,
        int wantedKolCount,
        decimal? otherFees = null)
    {
        var rewardSubtotal = rewardAmountPerKol * wantedKolCount;
        var openingFee = _settings.CaseOpeningFeeAmount;

        // 各項費用以報酬小計為計算基礎
        var kolServiceFee = Round(rewardSubtotal * _settings.KolServiceFeeRate / 100m);
        var platformFee = Round(rewardSubtotal * _settings.AffiliatePlatformCommissionRate / 100m);
        var taxAmount = Round(rewardSubtotal * _settings.KolTaxRate / 100m);
        var other = otherFees ?? 0m;

        var feeItems = new List<CaseFeeItem>
        {
            new() { Name = "案件開案費", Amount = openingFee },
            new() { Name = "KOL 服務費", Amount = kolServiceFee, Note = $"費率 {_settings.KolServiceFeeRate}%" },
            new() { Name = "平台手續費", Amount = platformFee, Note = $"費率 {_settings.AffiliatePlatformCommissionRate}%" },
            new() { Name = "稅金", Amount = taxAmount, Note = $"稅率 {_settings.KolTaxRate}%" }
        };

        if (other > 0)
        {
            feeItems.Add(new CaseFeeItem { Name = "其他費用", Amount = other });
        }

        var estimatedFrozenAmount = rewardSubtotal
            + openingFee
            + kolServiceFee
            + platformFee
            + taxAmount
            + other;

        return new CaseBudgetBreakdown
        {
            RewardAmountPerKol = rewardAmountPerKol,
            WantedKolCount = wantedKolCount,
            RewardSubtotal = rewardSubtotal,
            CaseOpeningFee = openingFee,
            KolServiceFee = kolServiceFee,
            PlatformFee = platformFee,
            TaxAmount = taxAmount,
            OtherFeesTotal = other,
            EstimatedFrozenAmount = estimatedFrozenAmount,
            FeeItemsJson = JsonSerializer.Serialize(feeItems, JsonOptions)
        };
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
