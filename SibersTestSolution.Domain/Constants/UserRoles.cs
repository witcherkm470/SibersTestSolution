namespace SibersTestSolution.Domain.Constants;

/// <summary>
/// Contains application role names used by ASP.NET Core Identity and authorization policies.
/// </summary>
public static class UserRoles
{
    /// <summary>
    /// Role for руководитель users with full access.
    /// </summary>
    public const string Head = "Head";

    /// <summary>
    /// Role for project managers who administer their assigned projects.
    /// </summary>
    public const string ProjectManager = "ProjectManager";

    /// <summary>
    /// Role for regular employees.
    /// </summary>
    public const string Employee = "Employee";

    /// <summary>
    /// Gets all supported role names.
    /// </summary>
    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Head,
        ProjectManager,
        Employee
    };
}
