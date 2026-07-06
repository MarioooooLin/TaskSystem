using System.Data;

namespace Application.Abstractions.Persistence;

/// <summary>
/// 資料庫 Session，封裝 IDbConnection + IDbTransaction。
/// Use Case 透過 IUnitOfWork 取得此 Session，再傳給各 Repository。
/// </summary>
public interface IDbSession : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
}
