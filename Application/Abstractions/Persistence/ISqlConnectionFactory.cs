using System.Data;

namespace Application.Abstractions.Persistence;

/// <summary>
/// MSSQL 連線工廠介面，由 Infrastructure 實作。
/// 使用 System.Data.IDbConnection 讓 Application 層不依賴 SqlClient 具體實作。
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// 建立並開啟一個新的資料庫連線。
    /// 呼叫端負責在 using 區塊中關閉連線。
    /// </summary>
    Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
