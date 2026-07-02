using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Projects;
using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Abstractions;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;
using Task = System.Threading.Tasks.Task;

namespace SibersTestSolution.Application.Services;

/// <summary>
/// Implements project management use cases.
/// </summary>
internal sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// Creates a project service.
    /// </summary>
    public ProjectService(
        IProjectRepository projectRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _unitOfWorkManager = unitOfWorkManager ?? throw new ArgumentNullException(nameof(unitOfWorkManager));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ProjectResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);

        return projects.Select(ProjectResponse.From).ToArray();
    }

    /// <inheritdoc />
    public async Task<ProjectResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetDetailedByIdAsync(id, cancellationToken);

        return project is null ? null : ProjectResponse.From(project);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();
        var project = new Project(
            request.Name,
            request.CustomerCompanyName,
            request.ContractorCompanyName,
            request.ProjectStartDate,
            request.ProjectEndDate,
            request.ProjectPriority);

        if (request.ProjectManagerId is not null)
        {
            var projectManager = await GetEmployeeOrThrowAsync(
                request.ProjectManagerId.Value,
                cancellationToken);

            project.AssignProjectManager(projectManager);
        }

        await AddEmployeesAsync(project, request.EmployeeIds, cancellationToken);
        await _projectRepository.AddAsync(project, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await GetProjectResponseOrThrowAsync(project.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> UpdateAsync(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();
        var project = await GetProjectForUpdateOrThrowAsync(id, cancellationToken);

        project.Rename(request.Name);
        project.ChangeCompanies(request.CustomerCompanyName, request.ContractorCompanyName);
        project.ChangePeriod(request.ProjectStartDate, request.ProjectEndDate);
        project.ChangePriority(request.ProjectPriority);

        await unitOfWork.CommitAsync(cancellationToken);

        return await GetProjectResponseOrThrowAsync(project.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkManager.Create();
        var project = await GetProjectForUpdateOrThrowAsync(id, cancellationToken);

        _projectRepository.Remove(project);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> AssignProjectManagerAsync(
        int projectId,
        AssignProjectManagerRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException($"Project with id '{projectId}' was not found.");
        }

        var projectManager = await GetEmployeeOrThrowAsync(request.EmployeeId, cancellationToken);

        project.AssignProjectManager(projectManager);
        await unitOfWork.CommitAsync(cancellationToken);

        return await GetProjectResponseOrThrowAsync(project.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> AddEmployeeAsync(
        int projectId,
        AddProjectEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();
        var project = await GetProjectForUpdateOrThrowAsync(projectId, cancellationToken);
        var employee = await GetEmployeeOrThrowAsync(request.EmployeeId, cancellationToken);

        project.AddEmployee(employee);

        await unitOfWork.CommitAsync(cancellationToken);

        return await GetProjectResponseOrThrowAsync(project.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProjectResponse> RemoveEmployeeAsync(
        int projectId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkManager.Create();
        var project = await GetProjectForUpdateOrThrowAsync(projectId, cancellationToken);

        project.RemoveEmployee(employeeId);

        await unitOfWork.CommitAsync(cancellationToken);

        return await GetProjectResponseOrThrowAsync(project.Id, cancellationToken);
    }

    private async Task AddEmployeesAsync(
        Project project,
        IReadOnlyCollection<int>? employeeIds,
        CancellationToken cancellationToken)
    {
        if (employeeIds is null || employeeIds.Count == 0)
        {
            return;
        }

        foreach (var employeeId in employeeIds.Distinct())
        {
            var employee = await GetEmployeeOrThrowAsync(employeeId, cancellationToken);
            project.AddEmployee(employee);
        }
    }

    private async Task<Employee> GetEmployeeOrThrowAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        if (employeeId <= 0)
        {
            throw new ArgumentException("Employee id must be greater than 0.", nameof(employeeId));
        }

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null || employee.IsDeleted)
        {
            throw new NotFoundException($"Employee with id '{employeeId}' was not found.");
        }

        return employee;
    }

    private async Task<ProjectResponse> GetProjectResponseOrThrowAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetDetailedByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException($"Project with id '{projectId}' was not found.");
        }

        return ProjectResponse.From(project);
    }

    private async Task<Project> GetProjectForUpdateOrThrowAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetDetailedForUpdateByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException($"Project with id '{projectId}' was not found.");
        }

        return project;
    }
}
