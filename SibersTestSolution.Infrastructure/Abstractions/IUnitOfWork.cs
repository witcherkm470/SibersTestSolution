namespace SibersTestSolution.Infrastructure.Abstractions;

/// <summary>
/// Represents a transactional unit of work for persistence operations.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the unit of work has been completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Saves pending changes without completing the transaction.
    /// </summary>
    int SaveChanges();

    /// <summary>
    /// Saves pending changes asynchronously without completing the transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes and commits the transaction.
    /// </summary>
    void Commit();

    /// <summary>
    /// Saves changes and commits the transaction asynchronously.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    void Rollback();

    /// <summary>
    /// Rolls back the transaction asynchronously.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
