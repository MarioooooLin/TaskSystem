using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.SystemSettings.DTOs;
using Common.Results;

namespace Application.SystemSettings.Queries;

public sealed class GetSystemSettingsHandler(
    IUnitOfWork unitOfWork,
    ISystemSettingRepository systemSettingRepo)
{
    public async Task<Result<SystemSettingValuesDto>> HandleAsync(
        GetSystemSettingsQuery _,
        CancellationToken ct = default)
    {
        await using var uow = await unitOfWork.BeginAsync(ct);

        var settings = await systemSettingRepo.GetAllAsync(uow.Session, ct);
        var map = settings.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        var dto = new SystemSettingValuesDto
        {
            CaseOpeningFeeAmount = GetDecimal(map, SystemSettingKeys.CaseOpeningFeeAmount),
            KolServiceFeeRate = GetDecimal(map, SystemSettingKeys.KolServiceFeeRate),
            AffiliatePlatformCommissionRate = GetDecimal(map, SystemSettingKeys.AffiliatePlatformCommissionRate),
            AffiliateKolMinCommissionRate = GetDecimal(map, SystemSettingKeys.AffiliateKolMinCommissionRate),
            CaseAutoExecutionThresholdRate = GetDecimal(map, SystemSettingKeys.CaseAutoExecutionThresholdRate, 50m),
            KolMinPayoutAmount = GetDecimal(map, SystemSettingKeys.KolMinPayoutAmount, 1000m),
            KolTaxRate = GetDecimal(map, SystemSettingKeys.KolTaxRate),
            KolPayoutFeeRate = GetDecimal(map, SystemSettingKeys.KolPayoutFeeRate),
            KolPayoutFixedFeeAmount = GetDecimal(map, SystemSettingKeys.KolPayoutFixedFeeAmount),
            KolPayoutMode = GetString(map, SystemSettingKeys.KolPayoutMode, "全額提領"),
            KolPayoutDays = GetString(map, SystemSettingKeys.KolPayoutDays, "10,25"),
            KolPayoutClosingDayOffset = GetInt(map, SystemSettingKeys.KolPayoutClosingDayOffset, -5),
        };

        await uow.CommitAsync(ct);
        return Result<SystemSettingValuesDto>.Success(dto);
    }

    private static decimal GetDecimal(IReadOnlyDictionary<string, Domain.Entities.SystemSetting> map, string key, decimal defaultValue = 0m)
    {
        if (map.TryGetValue(key, out var setting) && decimal.TryParse(setting.Value, out var value))
            return value;

        return defaultValue;
    }

    private static int GetInt(IReadOnlyDictionary<string, Domain.Entities.SystemSetting> map, string key, int defaultValue = 0)
    {
        if (map.TryGetValue(key, out var setting) && int.TryParse(setting.Value, out var value))
            return value;

        return defaultValue;
    }

    private static string GetString(IReadOnlyDictionary<string, Domain.Entities.SystemSetting> map, string key, string defaultValue)
    {
        if (map.TryGetValue(key, out var setting))
            return setting.Value;

        return defaultValue;
    }
}
