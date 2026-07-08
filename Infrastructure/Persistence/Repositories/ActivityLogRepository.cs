using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Dapper;

namespace Infrastructure.Persistence.Repositories;

public sealed class ActivityLogRepository : IActivityLogRepository
{
    public async Task WriteAsync(
        string targetType,
        long targetId,
        long? actorUserId,
        string action,
        string? note,
        IDbSession session,
        CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO ActivityLogs (TargetType, TargetId, ActorUserId, Action, Note)
            VALUES (@TargetType, @TargetId, @ActorUserId, @Action, @Note)
            """;

        await session.Connection.ExecuteAsync(sql, new
        {
            TargetType = targetType,
            TargetId = targetId,
            ActorUserId = actorUserId,
            Action = action,
            Note = note,
        }, session.Transaction);
    }
}
