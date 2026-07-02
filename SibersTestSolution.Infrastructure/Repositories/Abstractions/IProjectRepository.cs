using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Infrastructure.Repositories.Abstractions;

/// <summary>
/// Defines persistence operations for projects.
/// </summary>
public interface IProjectRepository : IEntityRepository<Project>
{
    /// <summary>
    /// Gets all projects with employee and manager data.
    /// </summary>
    Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project with related data by identifier.
    /// </summary>
    Task<Project?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked project with related data for updates.
    /// </summary>
    Task<Project?> GetDetailedForUpdateByIdAsync(int id, CancellationToken cancellationToken = default);
}
