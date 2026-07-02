using SibersTestSolution.Application.DTOs.Projects;

namespace SibersTestSolution.Application.Abstractions;

/// <summary>
/// Provides project use cases for controllers and other presentation layers.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets all projects with their manager and employee data.
    /// </summary>
    Task<IReadOnlyCollection<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by identifier.
    /// </summary>
    Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project.
    /// </summary>
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates project main data.
    /// </summary>
    Task<ProjectResponse> UpdateAsync(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project by identifier.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a project manager to the project.
    /// </summary>
    Task<ProjectResponse> AssignProjectManagerAsync(
        int projectId,
        AssignProjectManagerRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an employee to the project.
    /// </summary>
    Task<ProjectResponse> AddEmployeeAsync(
        int projectId,
        AddProjectEmployeeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an employee from the project.
    /// </summary>
    Task<ProjectResponse> RemoveEmployeeAsync(
        int projectId,
        int employeeId,
        CancellationToken cancellationToken = default);
}
