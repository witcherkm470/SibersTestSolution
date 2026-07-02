namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Query parameters for filtering and sorting employees.
/// </summary>
public sealed class EmployeeQueryParameters
{
    /// <summary>
    /// Gets or sets the legacy employee search text.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the employee search text.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Gets or sets the employee sort field.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public string? SortDirection { get; set; }
}
