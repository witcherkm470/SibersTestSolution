namespace SibersTestSolution.Application.DTOs.Projects;

/// <summary>
/// Request for updating project main data.
/// </summary>
public sealed record UpdateProjectRequest(
    string Name,
    string CustomerCompanyName,
    string ContractorCompanyName,
    DateTime ProjectStartDate,
    DateTime ProjectEndDate,
    int ProjectPriority);
