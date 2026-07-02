namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Query parameters for filtering and sorting projects.
/// </summary>
public sealed class ProjectQueryParameters
{
    /// <summary>
    /// Gets or sets the inclusive project start date lower bound.
    /// </summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>
    /// Gets or sets the inclusive project start date upper bound.
    /// </summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>
    /// Gets or sets the project priority filter.
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// Gets or sets the project search text.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the project sort field.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public string? SortDirection { get; set; }
}
