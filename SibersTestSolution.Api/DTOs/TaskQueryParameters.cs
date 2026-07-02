namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Query parameters for filtering and sorting tasks.
/// </summary>
public sealed class TaskQueryParameters
{
    /// <summary>
    /// Gets or sets the task status filter.
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Gets or sets the project identifier filter.
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the performer employee identifier filter.
    /// </summary>
    public int? PerformerId { get; set; }

    /// <summary>
    /// Gets or sets the owner employee identifier filter.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the task search text.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the task sort field.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public string? SortDirection { get; set; }
}
