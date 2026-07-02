namespace SibersTestSolution.Application.DTOs.Employees;

/// <summary>
/// Request for creating an employee and optionally issuing login credentials.
/// </summary>
public sealed record CreateEmployeeRequest(
    string Name,
    string LastName,
    string MiddleName,
    string Email,
    string? UserName = null,
    string? Password = null,
    string? Role = null);
