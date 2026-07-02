using Microsoft.EntityFrameworkCore;
using SibersTestSolution.Domain.Abstractions;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;

namespace SibersTestSolution.Infrastructure.Repositories;

/// <summary>
/// Base Entity Framework repository for common entity operations.
/// </summary>
public abstract class EntityRepository<TEntity, TDbContext> : IEntityRepository<TEntity>
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected readonly TDbContext DbContext;

    /// <summary>
    /// Gets the entity set.
    /// </summary>
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Creates a repository for the specified database context.
    /// </summary>
    protected EntityRepository(TDbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = DbContext.Set<TEntity>();
    }

    /// <inheritdoc />
    public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return DbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbSet.Remove(entity);
    }
}
