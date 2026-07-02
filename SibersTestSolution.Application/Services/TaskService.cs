using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Tasks;
using SibersTestSolution.Infrastructure.Abstractions;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;
using DomainTask = SibersTestSolution.Domain.Entities.Task;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Application.Services;

/// <summary>
/// Implements task management use cases.
/// </summary>
internal sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// Creates a task service.
    /// </summary>
    public TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _unitOfWorkManager = unitOfWorkManager ?? throw new ArgumentNullException(nameof(unitOfWorkManager));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<TaskResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);

        return tasks.Select(TaskResponse.From).ToArray();
    }

    /// <inheritdoc />
    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetDetailedByIdAsync(id, cancellationToken);

        return task is null ? null : TaskResponse.From(task);
    }

    /// <inheritdoc />
    public async Task<TaskResponse> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();

        await EnsureProjectExistsAsync(request.ProjectId, cancellationToken);
        await EnsureEmployeeExistsAsync(request.TaskOwnerId, "Task owner", cancellationToken);

        if (request.TaskPerformerId is not null)
        {
            await EnsureEmployeeExistsAsync(request.TaskPerformerId.Value, "Task performer", cancellationToken);
        }

        var task = new DomainTask(
            request.Name,
            request.ProjectId,
            request.TaskOwnerId,
            request.TaskPerformerId,
            request.Comment,
            request.TaskPriority);

        await _taskRepository.AddAsync(task, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await GetTaskResponseOrThrowAsync(task.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TaskResponse> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureValidTaskStatus(request.TaskStatus);

        using var unitOfWork = _unitOfWorkManager.Create();
        var task = await GetTaskOrThrowAsync(id, cancellationToken);

        await EnsureProjectExistsAsync(request.ProjectId, cancellationToken);
        await EnsureEmployeeExistsAsync(request.TaskOwnerId, "Task owner", cancellationToken);

        task.Rename(request.Name);
        task.ChangeProject(request.ProjectId);
        task.ChangeOwner(request.TaskOwnerId);

        if (request.TaskPerformerId is null)
        {
            task.RemovePerformer();
        }
        else
        {
            await EnsureEmployeeExistsAsync(request.TaskPerformerId.Value, "Task performer", cancellationToken);
            task.AssignPerformer(request.TaskPerformerId.Value);
        }

        task.ChangeStatus(request.TaskStatus);
        task.ChangeComment(request.Comment);
        task.ChangePriority(request.TaskPriority);

        await unitOfWork.CommitAsync(cancellationToken);

        return await GetTaskResponseOrThrowAsync(task.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TaskResponse> ChangeStatusAsync(
        int id,
        ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureValidTaskStatus(request.TaskStatus);

        using var unitOfWork = _unitOfWorkManager.Create();
        var task = await GetTaskOrThrowAsync(id, cancellationToken);

        task.ChangeStatus(request.TaskStatus);

        await unitOfWork.CommitAsync(cancellationToken);

        return await GetTaskResponseOrThrowAsync(task.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkManager.Create();
        var task = await GetTaskOrThrowAsync(id, cancellationToken);

        if (task.TaskStatus != TaskStatus.Todo)
        {
            throw new InvalidOperationException("Удалить можно только задачу со статусом Todo.");
        }

        _taskRepository.Remove(task);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<DomainTask> GetTaskOrThrowAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException($"Task with id '{id}' was not found.");
        }

        return task;
    }

    private async Task<TaskResponse> GetTaskResponseOrThrowAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetDetailedByIdAsync(id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException($"Task with id '{id}' was not found.");
        }

        return TaskResponse.From(task);
    }

    private async Task EnsureProjectExistsAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException($"Project with id '{projectId}' was not found.");
        }
    }

    private async Task EnsureEmployeeExistsAsync(
        int employeeId,
        string roleName,
        CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
        {
            throw new ArgumentException($"{roleName} id must be greater than 0.", nameof(employeeId));
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null || employee.IsDeleted)
        {
            throw new NotFoundException($"{roleName} with id '{employeeId}' was not found.");
        }
    }

    private static void EnsureValidTaskStatus(TaskStatus status)
    {
        if (!Enum.IsDefined(status) || status == TaskStatus.Default)
        {
            throw new ArgumentException("Task status has invalid value.", nameof(status));
        }
    }
}
