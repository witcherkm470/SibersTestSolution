using Microsoft.EntityFrameworkCore;
using SibersTestSolution.Infrastructure.Database;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;
using Task = SibersTestSolution.Domain.Entities.Task;

namespace SibersTestSolution.Infrastructure.Repositories;

/// <summary>
/// Entity Framework repository for tasks.
/// </summary>
public class TaskRepository : EntityRepository<Task, SibersTestSolutionDbContext>, ITaskRepository
{
    /// <summary>
    /// Creates a task repository.
    /// </summary>
    public TaskRepository(SibersTestSolutionDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Task>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.TaskOwner)
            .Include(x => x.TaskPerformer)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Task?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.TaskOwner)
            .Include(x => x.TaskPerformer)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
