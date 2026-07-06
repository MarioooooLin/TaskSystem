using Application.Abstractions.Persistence;
using System.Data;

namespace Infrastructure.Persistence.Transactions;

/// <summary>
/// IDbSession 實作，持有已開啟的連線與進行中的 Transaction。
/// </summary>
internal sealed class DbSession : IDbSession
{
    public IDbConnection Connection { get; }
    public IDbTransaction? Transaction { get; private set; }

    public DbSession(IDbConnection connection)
        => Connection = connection;

    internal void SetTransaction(IDbTransaction transaction)
        => Transaction = transaction;

    public async ValueTask DisposeAsync()
    {
        Transaction?.Dispose();
        if (Connection is IAsyncDisposable asyncConn)
            await asyncConn.DisposeAsync();
        else
            Connection.Dispose();
    }
}
