namespace SibersTestSolution.Application.DTOs.Employees;

/// <summary>
/// Request for updating employee data and optional login credentials.
/// </summary>
public sealed record UpdateEmployeeRequest(
    string Name,
    string LastName,
    string MiddleName,
    string Email,
    string? UserName = null,
    string? Password = null,
    string? Role = null);
