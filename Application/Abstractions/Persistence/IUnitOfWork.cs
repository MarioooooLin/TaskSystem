namespace Application.Abstractions.Persistence;

/// <summary>
/// 工作單元介面，封裝跨多個 Repository 的 Transaction 邊界。
///
/// 使用方式：
/// <code>
/// await using var uow = await _unitOfWork.BeginAsync();
/// await _caseRepo.UpdateAsync(caseEntity, uow.Session);
/// await _walletRepo.UpdateAsync(wallet, uow.Session);
/// await uow.CommitAsync();
/// </code>
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IDbSession Session { get; }

    /// <summary>開始 Transaction 並回傳 UnitOfWork 本身（供 await using 使用）。</summary>
    Task<IUnitOfWork> BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>提交 Transaction。</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>回滾 Transaction（DisposeAsync 時若未 Commit 也會自動呼叫）。</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
