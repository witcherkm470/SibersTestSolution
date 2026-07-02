using Task = SibersTestSolution.Domain.Entities.Task;

namespace SibersTestSolution.Infrastructure.Repositories.Abstractions;

/// <summary>
/// Defines persistence operations for tasks.
/// </summary>
public interface ITaskRepository : IEntityRepository<Task>
{
    /// <summary>
    /// Gets all tasks with project, owner, and performer data.
    /// </summary>
    Task<IReadOnlyCollection<Task>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task with related data by identifier.
    /// </summary>
    Task<Task?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default);
}
