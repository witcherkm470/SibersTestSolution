using SibersTestSolution.Domain.Abstractions;

namespace SibersTestSolution.Infrastructure.Repositories.Abstractions;

/// <summary>
/// Defines basic persistence operations for entities.
/// </summary>
public interface IEntityRepository<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an entity to the persistence context.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entity from the persistence context.
    /// </summary>
    void Remove(TEntity entity);
}
