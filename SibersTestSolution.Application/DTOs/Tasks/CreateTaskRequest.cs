namespace SibersTestSolution.Application.DTOs.Tasks;

/// <summary>
/// Request for creating a project task.
/// </summary>
public sealed record CreateTaskRequest(
    string Name,
    int ProjectId,
    int TaskOwnerId,
    int? TaskPerformerId,
    string? Comment,
    int TaskPriority);
