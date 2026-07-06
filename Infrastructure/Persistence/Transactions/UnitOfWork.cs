using Application.Abstractions.Persistence;

namespace Infrastructure.Persistence.Transactions;

/// <summary>
/// IUnitOfWork 實作。
///
/// 使用方式：
/// <code>
/// await using var uow = await _unitOfWork.BeginAsync();
/// await _caseRepo.UpdateAsync(entity, uow.Session);
/// await uow.CommitAsync();
/// // DisposeAsync 時若未 Commit，自動 Rollback
/// </code>
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private DbSession? _session;
    private bool _committed;

    public UnitOfWork(ISqlConnectionFactory connectionFactory)
        => _connectionFactory = connectionFactory;

    public IDbSession Session
        => _session ?? throw new InvalidOperationException("請先呼叫 BeginAsync() 再存取 Session。");

    public async Task<IUnitOfWork> BeginAsync(CancellationToken cancellationToken = default)
    {
        var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        _session = new DbSession(connection);

        var transaction = connection.BeginTransaction();
        _session.SetTransaction(transaction);

        _committed = false;
        return this;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_session?.Transaction is null)
            throw new InvalidOperationException("Transaction 尚未開始。");

        _session.Transaction.Commit();
        _committed = true;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _session?.Transaction?.Rollback();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
            await RollbackAsync();

        if (_session is not null)
            await _session.DisposeAsync();
    }
}
