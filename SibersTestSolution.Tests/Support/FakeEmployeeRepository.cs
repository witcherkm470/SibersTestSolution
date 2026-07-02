using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;

namespace SibersTestSolution.Tests.Support;

internal sealed class FakeEmployeeRepository : IEmployeeRepository
{
    private readonly Dictionary<int, Employee> _employees;
    private int _nextId;

    public FakeEmployeeRepository(params Employee[] employees)
    {
        _employees = employees.ToDictionary(x => x.Id);
        _nextId = _employees.Count == 0 ? 1 : _employees.Keys.Max() + 1;
    }

    public bool AddCalled { get; private set; }

    public bool RemoveCalled { get; private set; }

    public IReadOnlyCollection<Employee> Employees => _employees.Values.ToArray();

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _employees.TryGetValue(id, out var employee);
        return System.Threading.Tasks.Task.FromResult(employee);
    }

    public System.Threading.Tasks.Task AddAsync(Employee entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        AddCalled = true;

        if (entity.Id == 0)
        {
            entity.Id = _nextId++;
        }

        _employees[entity.Id] = entity;

        return System.Threading.Tasks.Task.CompletedTask;
    }

    public void Remove(Employee entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        RemoveCalled = true;
        _employees.Remove(entity.Id);
    }

    public Task<IReadOnlyCollection<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return System.Threading.Tasks.Task.FromResult<IReadOnlyCollection<Employee>>(_employees.Values.ToArray());
    }
}
