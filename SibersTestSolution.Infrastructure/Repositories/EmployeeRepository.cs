using Microsoft.EntityFrameworkCore;
using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Database;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;

namespace SibersTestSolution.Infrastructure.Repositories;

/// <summary>
/// Entity Framework repository for employees.
/// </summary>
public class EmployeeRepository : EntityRepository<Employee, SibersTestSolutionDbContext>, IEmployeeRepository
{
    /// <summary>
    /// Creates an employee repository.
    /// </summary>
    public EmployeeRepository(SibersTestSolutionDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
