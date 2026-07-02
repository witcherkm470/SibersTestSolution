using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Domain.Entities;

/// <summary>
/// Represents a project task with owner, optional performer, status, and priority.
/// </summary>
public class Task : Entity
{
    private Task()
    {
    }

    /// <summary>
    /// Creates a task in the <see cref="TaskStatus.Todo"/> status.
    /// </summary>
    public Task(
        string name,
        int projectId,
        int taskOwnerId,
        int? taskPerformerId,
        string? comment,
        int taskPriority)
    {
        SetName(name);
        SetProjectId(projectId);
        SetTaskOwnerId(taskOwnerId);
        SetTaskPerformerId(taskPerformerId);
        SetComment(comment);
        SetTaskPriority(taskPriority);

        ChangeStatus(TaskStatus.Todo);
    }

    /// <summary>
    /// Gets the task name.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the project identifier.
    /// </summary>
    public int ProjectId { get; private set; }

    /// <summary>
    /// Gets the project navigation property.
    /// </summary>
    public Project Project { get; private set; } = null!;

    /// <summary>
    /// Gets the task owner employee identifier.
    /// </summary>
    public int TaskOwnerId { get; private set; }

    /// <summary>
    /// Gets the task owner navigation property.
    /// </summary>
    public Employee TaskOwner { get; private set; } = null!;

    /// <summary>
    /// Gets the optional performer employee identifier.
    /// </summary>
    public int? TaskPerformerId { get; private set; }

    /// <summary>
    /// Gets the optional performer navigation property.
    /// </summary>
    public Employee? TaskPerformer { get; private set; }

    /// <summary>
    /// Gets the current task status.
    /// </summary>
    public TaskStatus TaskStatus { get; private set; }

    /// <summary>
    /// Gets an optional task comment.
    /// </summary>
    public string? Comment { get; private set; }

    /// <summary>
    /// Gets the task priority.
    /// </summary>
    public int TaskPriority { get; private set; }

    /// <summary>
    /// Renames the task.
    /// </summary>
    public void Rename(string name)
    {
        SetName(name);
    }

    /// <summary>
    /// Assigns an employee as the task performer.
    /// </summary>
    public void AssignPerformer(int performerId)
    {
        SetTaskPerformerId(performerId);
    }

    /// <summary>
    /// Moves the task to another project.
    /// </summary>
    public void ChangeProject(int projectId)
    {
        SetProjectId(projectId);
    }

    /// <summary>
    /// Changes the task owner.
    /// </summary>
    public void ChangeOwner(int taskOwnerId)
    {
        SetTaskOwnerId(taskOwnerId);
    }

    /// <summary>
    /// Removes the performer assignment from the task.
    /// </summary>
    public void RemovePerformer()
    {
        TaskPerformerId = null;
        TaskPerformer = null;
    }

    /// <summary>
    /// Changes the task status according to the allowed status transition rules.
    /// </summary>
    public void ChangeStatus(TaskStatus status)
    {
        if (!Enum.IsDefined(status) || status == TaskStatus.Default)
        {
            throw new ArgumentException("Task status has invalid value.", nameof(status));
        }

        if (TaskStatus == status)
        {
            return;
        }

        if (!CanChangeStatusTo(status))
        {
            throw new InvalidOperationException($"Task status cannot be changed from '{TaskStatus}' to '{status}'.");
        }

        TaskStatus = status;
    }

    /// <summary>
    /// Changes the task comment.
    /// </summary>
    public void ChangeComment(string? comment)
    {
        SetComment(comment);
    }

    /// <summary>
    /// Changes the task priority.
    /// </summary>
    public void ChangePriority(int priority)
    {
        SetTaskPriority(priority);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Task name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    private void SetProjectId(int projectId)
    {
        if (projectId <= 0)
        {
            throw new ArgumentException("Project id must be greater than 0.", nameof(projectId));
        }

        ProjectId = projectId;
    }

    private void SetTaskOwnerId(int taskOwnerId)
    {
        if (taskOwnerId <= 0)
        {
            throw new ArgumentException("Task owner id must be greater than 0.", nameof(taskOwnerId));
        }

        TaskOwnerId = taskOwnerId;
    }

    private void SetTaskPerformerId(int? taskPerformerId)
    {
        if (taskPerformerId is <= 0)
        {
            throw new ArgumentException("Task performer id must be greater than 0.", nameof(taskPerformerId));
        }

        TaskPerformerId = taskPerformerId;
    }

    private void SetComment(string? comment)
    {
        Comment = string.IsNullOrWhiteSpace(comment)
            ? null
            : comment.Trim();
    }

    private void SetTaskPriority(int taskPriority)
    {
        if (taskPriority <= 0)
        {
            throw new ArgumentException("Task priority must be greater than 0.", nameof(taskPriority));
        }

        TaskPriority = taskPriority;
    }

    private bool CanChangeStatusTo(TaskStatus status)
    {
        return TaskStatus switch
        {
            TaskStatus.Default => status == TaskStatus.Todo,
            TaskStatus.Todo => status == TaskStatus.InProgress,
            TaskStatus.InProgress => status is TaskStatus.Todo or TaskStatus.Done,
            _ => false
        };
    }
}
