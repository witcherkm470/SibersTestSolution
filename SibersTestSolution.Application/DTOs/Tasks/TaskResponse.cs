using SibersTestSolution.Application.DTOs.Employees;
using DomainTask = SibersTestSolution.Domain.Entities.Task;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Application.DTOs.Tasks;

/// <summary>
/// Response containing task data with project, owner, and performer details.
/// </summary>
public sealed record TaskResponse(
    int Id,
    string Name,
    int ProjectId,
    TaskProjectResponse? Project,
    int TaskOwnerId,
    EmployeeResponse? TaskOwner,
    int? TaskPerformerId,
    EmployeeResponse? TaskPerformer,
    TaskStatus TaskStatus,
    string? Comment,
    int TaskPriority)
{
    /// <summary>
    /// Maps a domain task to the response model.
    /// </summary>
    public static TaskResponse From(DomainTask task)
    {
        return new TaskResponse(
            task.Id,
            task.Name,
            task.ProjectId,
            task.Project is null ? null : TaskProjectResponse.From(task.Project),
            task.TaskOwnerId,
            task.TaskOwner is null ? null : EmployeeResponse.From(task.TaskOwner),
            task.TaskPerformerId,
            task.TaskPerformer is null ? null : EmployeeResponse.From(task.TaskPerformer),
            task.TaskStatus,
            task.Comment,
            task.TaskPriority);
    }
}
