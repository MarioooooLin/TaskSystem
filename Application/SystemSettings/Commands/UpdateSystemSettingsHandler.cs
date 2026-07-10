using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Security;
using Common.Results;
using Domain.Exceptions;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.SystemSettings.Commands;

public sealed class UpdateSystemSettingsHandler(
    IUnitOfWork unitOfWork,
    ISystemSettingRepository systemSettingRepo,
    IActivityLogRepository activityLogRepo,
    ICurrentUser currentUser)
{
    private static readonly Regex PayoutDaysRegex = new(
        @"^([1-9]|[12]\d|3[01])(,([1-9]|[12]\d|3[01]))*$",
        RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    public async Task<Result<IReadOnlyList<string>>> HandleAsync(
        UpdateSystemSettingsCommand cmd,
        CancellationToken ct = default)
    {
        var validation = Validate(cmd);
        if (validation.IsFailure)
            return Result.Failure<IReadOnlyList<string>>(validation.Error);

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [SystemSettingKeys.CaseOpeningFeeAmount] = cmd.CaseOpeningFeeAmount.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.KolServiceFeeRate] = cmd.KolServiceFeeRate.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.AffiliatePlatformCommissionRate] = cmd.AffiliatePlatformCommissionRate.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.AffiliateKolMinCommissionRate] = cmd.AffiliateKolMinCommissionRate.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.CaseAutoExecutionThresholdRate] = cmd.CaseAutoExecutionThresholdRate.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.KolMinPayoutAmount] = cmd.KolMinPayoutAmount.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.KolTaxRate] = cmd.KolTaxRate.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.KolPayoutFeeRate] = cmd.KolPayoutFeeRate.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.KolPayoutFixedFeeAmount] = cmd.KolPayoutFixedFeeAmount.ToString("G", CultureInfo.InvariantCulture),
            [SystemSettingKeys.KolPayoutMode] = cmd.KolPayoutMode.Trim(),
            [SystemSettingKeys.KolPayoutDays] = NormalizePayoutDays(cmd.KolPayoutDays),
            [SystemSettingKeys.KolPayoutClosingDayOffset] = cmd.KolPayoutClosingDayOffset.ToString(CultureInfo.InvariantCulture),
        };

        await using var uow = await unitOfWork.BeginAsync(ct);

        var currentSettings = await systemSettingRepo.GetAllAsync(uow.Session, ct);
        var missingKeys = updates.Keys.Except(currentSettings.Select(x => x.Key), StringComparer.OrdinalIgnoreCase).ToList();
        if (missingKeys.Count > 0)
        {
            return Result.Failure<IReadOnlyList<string>>(Errors.SystemSetting.NotFound);
        }

        var changedKeys = await systemSettingRepo.UpdateValuesAsync(
            updates,
            currentUser.UserId,
            cmd.Note,
            uow.Session,
            ct);

        if (changedKeys.Count > 0)
        {
            await activityLogRepo.WriteAsync(
                targetType: "SystemSettings",
                targetId: 0,
                actorUserId: currentUser.UserId,
                action: "UpdateSystemSettings",
                note: $"已更新參數：{string.Join(", ", changedKeys)}",
                session: uow.Session,
                ct: ct);
        }

        await uow.CommitAsync(ct);
        return Result.Success<IReadOnlyList<string>>(changedKeys);
    }

    private static Result Validate(UpdateSystemSettingsCommand cmd)
    {
        if (cmd.CaseOpeningFeeAmount < 0 ||
            cmd.KolMinPayoutAmount < 0 ||
            cmd.KolPayoutFixedFeeAmount < 0)
        {
            return Result.Failure(Errors.SystemSetting.InvalidValue);
        }

        if (cmd.KolServiceFeeRate is < 0 or > 100 ||
            cmd.AffiliatePlatformCommissionRate is < 0 or > 100 ||
            cmd.AffiliateKolMinCommissionRate is < 0 or > 100 ||
            cmd.CaseAutoExecutionThresholdRate is < 0 or > 100 ||
            cmd.KolTaxRate is < 0 or > 100 ||
            cmd.KolPayoutFeeRate is < 0 or > 100)
        {
            return Result.Failure(Errors.SystemSetting.InvalidValue);
        }

        if (cmd.AffiliatePlatformCommissionRate + cmd.AffiliateKolMinCommissionRate > 100)
        {
            return Result.Failure(Errors.SystemSetting.CommissionRateSumExceeded);
        }

        if (string.IsNullOrWhiteSpace(cmd.KolPayoutMode))
            return Result.Failure(Errors.SystemSetting.InvalidValue);

        if (!PayoutDaysRegex.IsMatch(cmd.KolPayoutDays.Trim()))
            return Result.Failure(Errors.SystemSetting.InvalidValue);

        return Result.Success();
    }

    private static string NormalizePayoutDays(string payoutDays)
    {
        var days = payoutDays.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x.Trim(), CultureInfo.InvariantCulture))
            .Distinct()
            .OrderBy(x => x);

        return string.Join(",", days);
    }
}
