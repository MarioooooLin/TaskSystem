using Application.Abstractions.Persistence;

namespace Application.Abstractions.Repositories;

public interface IActivityLogRepository
{
    /// <summary>
    /// 寫入一筆操作紀錄，必須在同一 transaction 內呼叫。
    /// </summary>
    Task WriteAsync(
        string targetType,
        long targetId,
        long? actorUserId,
        string action,
        string? note,
        IDbSession session,
        CancellationToken ct = default);
}
