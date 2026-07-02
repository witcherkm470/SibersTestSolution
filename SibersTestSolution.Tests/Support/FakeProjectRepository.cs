using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;

namespace SibersTestSolution.Tests.Support;

internal sealed class FakeProjectRepository : IProjectRepository
{
    private readonly Dictionary<int, Project> _projects;
    private int _nextId;

    public FakeProjectRepository(params Project[] projects)
    {
        _projects = projects.ToDictionary(x => x.Id);
        _nextId = _projects.Count == 0 ? 1 : _projects.Keys.Max() + 1;
    }

    public bool AddCalled { get; private set; }

    public bool RemoveCalled { get; private set; }

    public IReadOnlyCollection<Project> Projects => _projects.Values.ToArray();

    public Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _projects.TryGetValue(id, out var project);
        return System.Threading.Tasks.Task.FromResult(project);
    }

    public System.Threading.Tasks.Task AddAsync(Project entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        AddCalled = true;

        if (entity.Id == 0)
        {
            entity.Id = _nextId++;
        }

        _projects[entity.Id] = entity;

        return System.Threading.Tasks.Task.CompletedTask;
    }

    public void Remove(Project entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        RemoveCalled = true;
        _projects.Remove(entity.Id);
    }

    public Task<IReadOnlyCollection<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return System.Threading.Tasks.Task.FromResult<IReadOnlyCollection<Project>>(_projects.Values.ToArray());
    }

    public Task<Project?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(id, cancellationToken);
    }

    public Task<Project?> GetDetailedForUpdateByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(id, cancellationToken);
    }
}
