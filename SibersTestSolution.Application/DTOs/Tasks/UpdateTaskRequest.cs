using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Application.DTOs.Tasks;

/// <summary>
/// Request for updating task data.
/// </summary>
public sealed record UpdateTaskRequest(
    string Name,
    int ProjectId,
    int TaskOwnerId,
    int? TaskPerformerId,
    TaskStatus TaskStatus,
    string? Comment,
    int TaskPriority);
