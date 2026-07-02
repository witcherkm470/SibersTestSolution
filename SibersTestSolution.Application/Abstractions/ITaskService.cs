using SibersTestSolution.Application.DTOs.Tasks;

namespace SibersTestSolution.Application.Abstractions;

/// <summary>
/// Provides task use cases for controllers and other presentation layers.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Gets all tasks with project and employee data.
    /// </summary>
    Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by identifier.
    /// </summary>
    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new task.
    /// </summary>
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task data.
    /// </summary>
    Task<TaskResponse> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the task status according to domain transition rules.
    /// </summary>
    Task<TaskResponse> ChangeStatusAsync(
        int id,
        ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task by identifier.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
