namespace SibersTestSolution.Application.DTOs.Projects;

/// <summary>
/// Request for assigning an employee as project manager.
/// </summary>
public sealed record AssignProjectManagerRequest(int EmployeeId);
