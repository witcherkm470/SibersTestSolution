using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SibersTestSolution.Infrastructure.Internal;

/// <summary>
/// Adapts an EF Core database context for the unit-of-work infrastructure.
/// </summary>
internal sealed class UnitOfWorkDbContext<TDbContext> : IUnitOfWorkDbContext where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;

    /// <summary>
    /// Creates a unit-of-work database context adapter.
    /// </summary>
    public UnitOfWorkDbContext(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public IDbContextTransaction BeginTransaction()
    {
        return _dbContext.Database.BeginTransaction();
    }
}
