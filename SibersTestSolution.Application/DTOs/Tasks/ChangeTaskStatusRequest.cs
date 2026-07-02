using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Application.DTOs.Tasks;

/// <summary>
/// Request for changing a task status.
/// </summary>
public sealed record ChangeTaskStatusRequest(TaskStatus TaskStatus);
