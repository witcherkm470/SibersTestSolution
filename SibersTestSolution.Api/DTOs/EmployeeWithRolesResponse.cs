using SibersTestSolution.Application.DTOs.Employees;

namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Employee data enriched with authentication roles.
/// </summary>
public sealed record EmployeeWithRolesResponse(
    int Id,
    string Name,
    string LastName,
    string MiddleName,
    string Email,
    bool IsDeleted,
    string? UserName,
    IReadOnlyCollection<string> Roles)
{
    /// <summary>
    /// Creates a response from application employee data and Identity roles.
    /// </summary>
    public static EmployeeWithRolesResponse From(
        EmployeeResponse employee,
        string? userName,
        IReadOnlyCollection<string> roles)
    {
        return new EmployeeWithRolesResponse(
            employee.Id,
            employee.Name,
            employee.LastName,
            employee.MiddleName,
            employee.Email,
            employee.IsDeleted,
            userName,
            roles);
    }
}
