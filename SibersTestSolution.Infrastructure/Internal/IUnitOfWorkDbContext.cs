using Microsoft.EntityFrameworkCore.Storage;

namespace SibersTestSolution.Infrastructure.Internal;

/// <summary>
/// Minimal database context contract required by the unit-of-work implementation.
/// </summary>
internal interface IUnitOfWorkDbContext
{
    /// <summary>
    /// Saves pending changes.
    /// </summary>
    int SaveChanges();

    /// <summary>
    /// Saves pending changes asynchronously.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    IDbContextTransaction BeginTransaction();
}
