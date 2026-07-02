using Microsoft.EntityFrameworkCore;
using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Database;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;

namespace SibersTestSolution.Infrastructure.Repositories;

/// <summary>
/// Entity Framework repository for projects.
/// </summary>
public class ProjectRepository : EntityRepository<Project, SibersTestSolutionDbContext>, IProjectRepository
{
    /// <summary>
    /// Creates a project repository.
    /// </summary>
    public ProjectRepository(SibersTestSolutionDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.Employees)
            .Include(x => x.ProjectManager)
            .Include(x => x.Documents)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Project?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking()
            .Include(x => x.Employees)
            .Include(x => x.ProjectManager)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Project?> GetDetailedForUpdateByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbSet
            .Include(x => x.Employees)
            .Include(x => x.ProjectManager)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
