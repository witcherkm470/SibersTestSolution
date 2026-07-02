using SibersTestSolution.Application.DTOs.Employees;

namespace SibersTestSolution.Application.DTOs.Projects;

/// <summary>
/// Response containing project data with assigned employees and manager.
/// </summary>
public sealed record ProjectResponse(
    int Id,
    string Name,
    string CustomerCompanyName,
    string ContractorCompanyName,
    IReadOnlyCollection<EmployeeResponse> Employees,
    int? ProjectManagerId,
    EmployeeResponse? ProjectManager,
    DateTime ProjectStartDate,
    DateTime ProjectEndDate,
    int ProjectPriority,
    IReadOnlyCollection<ProjectDocumentInfoResponse> Documents)
{
    /// <summary>
    /// Maps a domain project to the response model.
    /// </summary>
    public static ProjectResponse From(Domain.Entities.Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.Name,
            project.CustomerCompanyName,
            project.ContractorCompanyName,
            project.Employees.Select(EmployeeResponse.From).ToArray(),
            project.ProjectManagerId,
            project.ProjectManager is null ? null : EmployeeResponse.From(project.ProjectManager),
            project.ProjectStartDate,
            project.ProjectEndDate,
            project.ProjectPriority,
            project.Documents
                .OrderByDescending(document => document.UploadedAtUtc)
                .Select(ProjectDocumentInfoResponse.From)
                .ToArray());
    }
}
