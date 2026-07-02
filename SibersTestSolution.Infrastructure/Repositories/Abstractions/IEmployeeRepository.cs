using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Infrastructure.Repositories.Abstractions;

/// <summary>
/// Defines persistence operations for employees.
/// </summary>
public interface IEmployeeRepository : IEntityRepository<Employee>
{
    /// <summary>
    /// Gets all employees.
    /// </summary>
    Task<IReadOnlyCollection<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
}
