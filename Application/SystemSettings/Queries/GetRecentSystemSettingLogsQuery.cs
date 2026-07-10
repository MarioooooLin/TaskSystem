namespace Application.SystemSettings.Queries;

/// <summary>取得最近 N 筆系統參數異動紀錄。</summary>
public sealed record GetRecentSystemSettingLogsQuery(int Count = 10);
