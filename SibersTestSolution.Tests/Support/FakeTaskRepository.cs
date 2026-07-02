using SibersTestSolution.Infrastructure.Repositories.Abstractions;
using DomainTask = SibersTestSolution.Domain.Entities.Task;

namespace SibersTestSolution.Tests.Support;

internal sealed class FakeTaskRepository : ITaskRepository
{
    private readonly Dictionary<int, DomainTask> _tasks;
    private int _nextId;

    public FakeTaskRepository(params DomainTask[] tasks)
    {
        _tasks = tasks.ToDictionary(x => x.Id);
        _nextId = _tasks.Count == 0 ? 1 : _tasks.Keys.Max() + 1;
    }

    public bool AddCalled { get; private set; }

    public bool RemoveCalled { get; private set; }

    public IReadOnlyCollection<DomainTask> Tasks => _tasks.Values.ToArray();

    public Task<DomainTask?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _tasks.TryGetValue(id, out var task);
        return Task.FromResult(task);
    }

    public Task AddAsync(DomainTask entity, CancellationToken cancellationToken = default)
    {
        AddCalled = true;

        if (entity.Id == 0)
        {
            entity.Id = _nextId++;
        }

        _tasks[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public void Remove(DomainTask entity)
    {
        RemoveCalled = true;
        _tasks.Remove(entity.Id);
    }

    public Task<IReadOnlyCollection<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<DomainTask>>(_tasks.Values.ToArray());
    }

    public Task<DomainTask?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return GetByIdAsync(id, cancellationToken);
    }
}
