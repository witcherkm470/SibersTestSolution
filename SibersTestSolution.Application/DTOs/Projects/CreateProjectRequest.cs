namespace SibersTestSolution.Application.DTOs.Projects;

/// <summary>
/// Request for creating a project with optional manager and employees.
/// </summary>
public sealed record CreateProjectRequest(
    string Name,
    string CustomerCompanyName,
    string ContractorCompanyName,
    DateTime ProjectStartDate,
    DateTime ProjectEndDate,
    int ProjectPriority,
    int? ProjectManagerId,
    IReadOnlyCollection<int>? EmployeeIds);
