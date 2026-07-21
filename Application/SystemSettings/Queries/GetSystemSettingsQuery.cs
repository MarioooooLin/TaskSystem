using Application.Abstractions.Persistence;

namespace Application.SystemSettings.Queries;

/// <summary>取得目前系統參數設定值。</summary>
/// <param name="Session">若提供外部 IDbSession，則不自行開啟 UnitOfWork。</param>
public sealed record GetSystemSettingsQuery(IDbSession? Session = null);
