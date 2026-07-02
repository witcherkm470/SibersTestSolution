namespace SibersTestSolution.Domain.Enums;

/// <summary>
/// Defines lifecycle statuses for project tasks.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Empty value used only as a guard against invalid input.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The task is planned but not yet started.
    /// </summary>
    Todo = 1,

    /// <summary>
    /// The task is currently in progress.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// The task is finished and cannot change status again.
    /// </summary>
    Done = 3
}
