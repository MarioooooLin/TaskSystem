using Application.Abstractions.Persistence;
using Application.SystemSettings.DTOs;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

/// <summary>系統參數與異動紀錄 Repository。</summary>
public interface ISystemSettingRepository
{
    /// <summary>取得所有系統參數。</summary>
    Task<IReadOnlyList<SystemSetting>> GetAllAsync(
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>依 Key 取得單一參數。</summary>
    Task<SystemSetting?> GetByKeyAsync(
        string key,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>
    /// 批次更新有異動的參數，並同步寫入 SystemSettingLogs。
    /// 只更新 <paramref name="updates"/> 中包含且值與目前不同的 Key。
    /// </summary>
    /// <returns>實際異動的 Key 清單。</returns>
    Task<IReadOnlyList<string>> UpdateValuesAsync(
        IReadOnlyDictionary<string, string> updates,
        long? updatedByUserId,
        string? note,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>
    /// 將所有參數還原為 DefaultValue，並同步寫入 SystemSettingLogs。
    /// </summary>
    /// <returns>實際異動的 Key 清單。</returns>
    Task<IReadOnlyList<string>> ResetToDefaultsAsync(
        long? updatedByUserId,
        string? note,
        IDbSession session,
        CancellationToken ct = default);

    /// <summary>取得最近 N 筆異動紀錄。</summary>
    Task<IReadOnlyList<SystemSettingLogDto>> GetRecentLogsAsync(
        int count,
        IDbSession session,
        CancellationToken ct = default);
}
