using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Application.DTOs.Employees;

/// <summary>
/// Response containing employee profile data.
/// </summary>
public sealed record EmployeeResponse(
    int Id,
    string Name,
    string LastName,
    string MiddleName,
    string Email,
    bool IsDeleted)
{
    /// <summary>
    /// Maps a domain employee to the response model.
    /// </summary>
    public static EmployeeResponse From(Employee employee)
    {
        return new EmployeeResponse(
            employee.Id,
            employee.Name,
            employee.LastName,
            employee.MiddleName,
            employee.Email,
            employee.IsDeleted);
    }
}
