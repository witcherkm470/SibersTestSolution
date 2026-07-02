using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SibersTestSolution.Api.DTOs;
using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Projects;
using SibersTestSolution.Domain.Constants;
using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Database;
using SibersTestSolution.Infrastructure.Identity;
using SibersTestSolution.Infrastructure.Options;

namespace SibersTestSolution.Api.Controllers;

/// <summary>
/// Provides project management endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SibersTestSolutionDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly ProjectDocumentOptions _documentOptions;

    /// <summary>
    /// Creates a projects controller.
    /// </summary>
    public ProjectsController(
        IProjectService projectService,
        UserManager<ApplicationUser> userManager,
        SibersTestSolutionDbContext dbContext,
        IWebHostEnvironment environment,
        IOptions<ProjectDocumentOptions> documentOptions)
    {
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _documentOptions = documentOptions.Value ?? throw new ArgumentNullException(nameof(documentOptions));
    }

    /// <summary>
    /// Gets projects visible to the current user with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectResponse>>> GetAll(
        [FromQuery] ProjectQueryParameters query,
        CancellationToken cancellationToken)
    {
        var employeeId = await GetCurrentEmployeeIdAsync();

        if (!User.IsInRole(UserRoles.Head) && employeeId is null)
        {
            return Forbid();
        }

        var response = ApplyProjectQuery(
            FilterProjectsByAccess(await _projectService.GetAllAsync(cancellationToken), employeeId),
            query);

        return Ok(response);
    }

    /// <summary>
    /// Gets a project by identifier if it is visible to the current user.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _projectService.GetByIdAsync(id, cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        if (!CanViewProject(response, await GetCurrentEmployeeIdAsync()))
        {
            return Forbid();
        }

        return Ok(response);
    }

    /// <summary>
    /// Creates a project.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await CanAssignProjectManagerAsync(request.ProjectManagerId))
            {
                return BadRequest(new ErrorResponse("Only employee with ProjectManager role can be assigned as project manager."));
            }

            var response = await _projectService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Updates project main data.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> Update(
        int id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _projectService.UpdateAsync(id, request, cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Deletes a project by identifier.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _projectService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Assigns an employee with the project manager role as project manager.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpPatch("{id:int}/manager")]
    public async Task<ActionResult<ProjectResponse>> AssignProjectManager(int id, AssignProjectManagerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await CanAssignProjectManagerAsync(request.EmployeeId))
            {
                return BadRequest(new ErrorResponse("Only employee with ProjectManager role can be assigned as project manager."));
            }

            var response = await _projectService.AssignProjectManagerAsync(id, request, cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Adds an employee to a project managed by the current user.
    /// </summary>
    [HttpPost("{id:int}/employees")]
    public async Task<ActionResult<ProjectResponse>> AddEmployee(
        int id,
        AddProjectEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await CanManageProjectEmployeesAsync(id, cancellationToken))
            {
                return Forbid();
            }

            var response = await _projectService.AddEmployeeAsync(id, request, cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Removes an employee from a project managed by the current user.
    /// </summary>
    [HttpDelete("{id:int}/employees/{employeeId:int}")]
    public async Task<ActionResult<ProjectResponse>> RemoveEmployee(
        int id,
        int employeeId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await CanManageProjectEmployeesAsync(id, cancellationToken))
            {
                return Forbid();
            }

            var response = await _projectService.RemoveEmployeeAsync(id, employeeId, cancellationToken);

            return Ok(response);
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Uploads documents for a project and replaces previously uploaded files.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpPost("{id:int}/documents")]
    public async Task<ActionResult<IReadOnlyCollection<ProjectDocumentResponse>>> UploadDocuments(
        int id,
        [FromForm] List<IFormFile> files,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return NotFound(new ErrorResponse($"Project with id '{id}' was not found."));
        }

        if (files.Count == 0)
        {
            return BadRequest(new ErrorResponse("At least one project document is required."));
        }

        var acceptedFiles = files
            .Where(file => file.Length > 0)
            .ToArray();

        foreach (var file in acceptedFiles)
        {
            if (file.Length > _documentOptions.MaxFileSizeBytes)
            {
                return BadRequest(new ErrorResponse($"File '{file.FileName}' is larger than allowed limit."));
            }
        }

        if (acceptedFiles.Length == 0)
        {
            return BadRequest(new ErrorResponse("Uploaded files are empty."));
        }

        var storageRoot = GetDocumentStorageRoot();
        var projectDirectory = Path.Combine(storageRoot, id.ToString());
        Directory.CreateDirectory(projectDirectory);

        var documents = new List<ProjectDocument>();
        var storedFilePaths = new List<string>();

        try
        {
            foreach (var file in acceptedFiles)
            {
                var originalFileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(originalFileName);
                var storedFileName = $"{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(projectDirectory, storedFileName);
                var relativePath = Path.Combine(id.ToString(), storedFileName);

                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                storedFilePaths.Add(fullPath);
                documents.Add(new ProjectDocument(
                    id,
                    originalFileName,
                    storedFileName,
                    file.ContentType,
                    file.Length,
                    relativePath));
            }

            var existingDocuments = await _dbContext.ProjectDocuments
                .Where(document => document.ProjectId == id)
                .ToListAsync(cancellationToken);

            _dbContext.ProjectDocuments.RemoveRange(existingDocuments);
            await _dbContext.ProjectDocuments.AddRangeAsync(documents, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var existingDocument in existingDocuments)
            {
                DeleteProjectDocumentFile(existingDocument);
            }
        }
        catch
        {
            foreach (var storedFilePath in storedFilePaths)
            {
                DeleteFileIfExists(storedFilePath);
            }

            throw;
        }

        return Ok(documents.Select(ProjectDocumentResponse.From).ToArray());
    }

    /// <summary>
    /// Downloads a project document when the project is visible to the current user.
    /// </summary>
    [HttpGet("{id:int}/documents/{documentId:int}/download")]
    public async Task<IActionResult> DownloadDocument(
        int id,
        int documentId,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return NotFound(new ErrorResponse($"Project with id '{id}' was not found."));
        }

        if (!CanViewProject(project, await GetCurrentEmployeeIdAsync()))
        {
            return Forbid();
        }

        var document = await _dbContext.ProjectDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == documentId && item.ProjectId == id,
                cancellationToken);

        if (document is null)
        {
            return NotFound(new ErrorResponse($"Document with id '{documentId}' was not found."));
        }

        var fullPath = GetProjectDocumentFullPath(document);

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound(new ErrorResponse("Document file was not found on disk."));
        }

        return PhysicalFile(
            fullPath,
            string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            document.OriginalFileName);
    }

    private IReadOnlyCollection<ProjectResponse> FilterProjectsByAccess(
        IReadOnlyCollection<ProjectResponse> projects,
        int? employeeId)
    {
        if (User.IsInRole(UserRoles.Head))
        {
            return projects;
        }

        if (employeeId is null)
        {
            return Array.Empty<ProjectResponse>();
        }

        return projects
            .Where(project => CanViewProject(project, employeeId))
            .ToArray();
    }

    private bool CanViewProject(ProjectResponse project, int? employeeId)
    {
        if (User.IsInRole(UserRoles.Head))
        {
            return true;
        }

        if (employeeId is null)
        {
            return false;
        }

        if (User.IsInRole(UserRoles.ProjectManager) && project.ProjectManagerId == employeeId)
        {
            return true;
        }

        return project.Employees.Any(employee => employee.Id == employeeId);
    }

    private async Task<bool> CanManageProjectEmployeesAsync(
        int projectId,
        CancellationToken cancellationToken)
    {
        if (User.IsInRole(UserRoles.Head))
        {
            return true;
        }

        if (!User.IsInRole(UserRoles.ProjectManager))
        {
            return false;
        }

        var employeeId = await GetCurrentEmployeeIdAsync();

        if (employeeId is null)
        {
            return false;
        }

        var project = await _projectService.GetByIdAsync(projectId, cancellationToken);

        return project?.ProjectManagerId == employeeId;
    }

    private async Task<int?> GetCurrentEmployeeIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return user?.EmployeeId;
    }

    private async Task<bool> CanAssignProjectManagerAsync(int? employeeId)
    {
        if (employeeId is null)
        {
            return true;
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

        return user is not null && await _userManager.IsInRoleAsync(user, UserRoles.ProjectManager);
    }

    private static IReadOnlyCollection<ProjectResponse> ApplyProjectQuery(
        IReadOnlyCollection<ProjectResponse> projects,
        ProjectQueryParameters query)
    {
        IEnumerable<ProjectResponse> result = projects;

        if (query.StartDateFrom is not null)
        {
            result = result.Where(project => project.ProjectStartDate.Date >= query.StartDateFrom.Value.Date);
        }

        if (query.StartDateTo is not null)
        {
            result = result.Where(project => project.ProjectStartDate.Date <= query.StartDateTo.Value.Date);
        }

        if (query.Priority is not null)
        {
            result = result.Where(project => project.ProjectPriority == query.Priority);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();

            result = result.Where(project =>
                $"{project.Name} {project.CustomerCompanyName} {project.ContractorCompanyName}".ToLowerInvariant().Contains(search));
        }

        result = SortProjects(result, query.SortBy, query.SortDirection);

        return result.ToArray();
    }

    private static IEnumerable<ProjectResponse> SortProjects(
        IEnumerable<ProjectResponse> projects,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? projects.OrderByDescending(x => x.Name) : projects.OrderBy(x => x.Name),
            "customer" => descending ? projects.OrderByDescending(x => x.CustomerCompanyName) : projects.OrderBy(x => x.CustomerCompanyName),
            "contractor" => descending ? projects.OrderByDescending(x => x.ContractorCompanyName) : projects.OrderBy(x => x.ContractorCompanyName),
            "startdate" => descending ? projects.OrderByDescending(x => x.ProjectStartDate) : projects.OrderBy(x => x.ProjectStartDate),
            "enddate" => descending ? projects.OrderByDescending(x => x.ProjectEndDate) : projects.OrderBy(x => x.ProjectEndDate),
            "priority" => descending ? projects.OrderByDescending(x => x.ProjectPriority) : projects.OrderBy(x => x.ProjectPriority),
            _ => projects.OrderBy(x => x.Id)
        };
    }

    private string GetDocumentStorageRoot()
    {
        if (Path.IsPathRooted(_documentOptions.StoragePath))
        {
            return _documentOptions.StoragePath;
        }

        return Path.Combine(_environment.ContentRootPath, _documentOptions.StoragePath);
    }

    private string GetProjectDocumentFullPath(ProjectDocument document)
    {
        var storageRoot = Path.GetFullPath(GetDocumentStorageRoot());
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, document.RelativePath));
        var normalizedStorageRoot = storageRoot.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(normalizedStorageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Document path is outside of configured storage.");
        }

        return fullPath;
    }

    private void DeleteProjectDocumentFile(ProjectDocument document)
    {
        DeleteFileIfExists(GetProjectDocumentFullPath(document));
    }

    private static void DeleteFileIfExists(string fullPath)
    {
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
