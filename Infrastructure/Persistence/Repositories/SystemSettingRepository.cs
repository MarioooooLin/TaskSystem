using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.SystemSettings;
using Application.SystemSettings.DTOs;
using Dapper;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public sealed class SystemSettingRepository : ISystemSettingRepository
{
    public async Task<IReadOnlyList<SystemSetting>> GetAllAsync(
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, [Key], Value, DefaultValue, ValueType, [Group], Description, UpdatedByUserId, UpdatedAt
            FROM SystemSettings
            ORDER BY [Group], [Key]
            """;

        var items = await session.Connection.QueryAsync<SystemSetting>(sql, transaction: session.Transaction);
        return items.AsList();
    }

    public async Task<SystemSetting?> GetByKeyAsync(
        string key,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT Id, [Key], Value, DefaultValue, ValueType, [Group], Description, UpdatedByUserId, UpdatedAt
            FROM SystemSettings
            WHERE [Key] = @Key
            """;

        return await session.Connection.QueryFirstOrDefaultAsync<SystemSetting>(
            sql,
            new { Key = key },
            session.Transaction);
    }

    public async Task<IReadOnlyList<string>> UpdateValuesAsync(
        IReadOnlyDictionary<string, string> updates,
        long? updatedByUserId,
        string? note,
        IDbSession session,
        CancellationToken ct = default)
    {
        var current = await GetAllAsync(session, ct);
        var currentMap = current.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        var changedKeys = new List<string>();

        foreach (var (key, newValue) in updates)
        {
            if (!currentMap.TryGetValue(key, out var setting))
                continue;

            if (string.Equals(setting.Value, newValue, StringComparison.Ordinal))
                continue;

            await UpdateSingleAsync(setting, newValue, updatedByUserId, note, session, ct);
            changedKeys.Add(key);
        }

        return changedKeys;
    }

    public async Task<IReadOnlyList<string>> ResetToDefaultsAsync(
        long? updatedByUserId,
        string? note,
        IDbSession session,
        CancellationToken ct = default)
    {
        var current = await GetAllAsync(session, ct);
        var changedSettings = current
            .Where(x => !string.Equals(x.Value, x.DefaultValue, StringComparison.Ordinal))
            .ToList();

        foreach (var setting in changedSettings)
        {
            await UpdateValueAsync(setting, setting.DefaultValue!, updatedByUserId, session, ct);
        }

        if (changedSettings.Count > 0)
        {
            await InsertLogAsync(
                SystemSettingKeys.ResetAll,
                oldValue: null,
                newValue: "已還原為系統預設值",
                note,
                updatedByUserId,
                session,
                ct);
        }

        return changedSettings.Select(x => x.Key).ToList();
    }

    public async Task<IReadOnlyList<SystemSettingLogDto>> GetRecentLogsAsync(
        int count,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT TOP (@Count)
                l.SettingKey,
                ISNULL(u.Name, '系統') AS ChangedByName,
                l.OldValue,
                l.NewValue,
                l.Note,
                l.ChangedAt
            FROM SystemSettingLogs l
            LEFT JOIN Users u ON u.Id = l.ChangedByUserId
            ORDER BY l.ChangedAt DESC, l.Id DESC
            """;

        var rows = await session.Connection.QueryAsync<SystemSettingLogRaw>(
            sql,
            new { Count = count },
            session.Transaction);

        return rows.Select(x => new SystemSettingLogDto
        {
            SettingKey = x.SettingKey,
            SettingName = SystemSettingKeys.Names.TryGetValue(x.SettingKey, out var name) ? name : x.SettingKey,
            OldValue = x.OldValue,
            NewValue = x.NewValue,
            ChangedByName = x.ChangedByName,
            ChangedAt = x.ChangedAt,
            Note = x.Note,
        }).AsList();
    }

    private static async Task UpdateSingleAsync(
        SystemSetting setting,
        string newValue,
        long? updatedByUserId,
        string? note,
        IDbSession session,
        CancellationToken ct)
    {
        await UpdateValueAsync(setting, newValue, updatedByUserId, session, ct);

        await InsertLogAsync(
            setting.Key,
            setting.Value,
            newValue,
            note,
            updatedByUserId,
            session,
            ct);
    }

    private static async Task UpdateValueAsync(
        SystemSetting setting,
        string newValue,
        long? updatedByUserId,
        IDbSession session,
        CancellationToken ct)
    {
        const string updateSql = """
            UPDATE SystemSettings
            SET Value = @Value,
                UpdatedByUserId = @UpdatedByUserId,
                UpdatedAt = GETUTCDATE()
            WHERE [Key] = @Key
            """;

        await session.Connection.ExecuteAsync(
            updateSql,
            new
            {
                Key = setting.Key,
                Value = newValue,
                UpdatedByUserId = updatedByUserId,
            },
            session.Transaction);
    }

    private static async Task InsertLogAsync(
        string settingKey,
        string? oldValue,
        string newValue,
        string? note,
        long? changedByUserId,
        IDbSession session,
        CancellationToken ct)
    {
        const string logSql = """
            INSERT INTO SystemSettingLogs (SettingKey, OldValue, NewValue, Note, ChangedByUserId)
            VALUES (@SettingKey, @OldValue, @NewValue, @Note, @ChangedByUserId)
            """;

        await session.Connection.ExecuteAsync(
            logSql,
            new
            {
                SettingKey = settingKey,
                OldValue = oldValue,
                NewValue = newValue,
                Note = note,
                ChangedByUserId = changedByUserId,
            },
            session.Transaction);
    }

    private sealed class SystemSettingLogRaw
    {
        public string SettingKey { get; set; } = string.Empty;
        public string? ChangedByName { get; set; }
        public string? OldValue { get; set; }
        public string NewValue { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
