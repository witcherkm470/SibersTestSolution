using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SibersTestSolution.Api.DTOs;
using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Tasks;
using SibersTestSolution.Domain.Constants;
using SibersTestSolution.Infrastructure.Identity;

namespace SibersTestSolution.Api.Controllers;

/// <summary>
/// Provides task management endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Creates a tasks controller.
    /// </summary>
    public TasksController(
        ITaskService taskService,
        IProjectService projectService,
        UserManager<ApplicationUser> userManager)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Gets tasks visible to the current user with optional filtering and sorting.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TaskResponse>>> GetAll(
        [FromQuery] TaskQueryParameters query,
        CancellationToken cancellationToken)
    {
        var employeeId = await GetCurrentEmployeeIdAsync();

        if (!User.IsInRole(UserRoles.Head) && employeeId is null)
        {
            return Forbid();
        }

        var response = ApplyTaskQuery(
            FilterTasksByAccess(await _taskService.GetAllAsync(cancellationToken), employeeId),
            query);

        return Ok(response);
    }

    /// <summary>
    /// Gets a task by identifier if it is visible to the current user.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _taskService.GetByIdAsync(id, cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        if (!CanViewTask(response, await GetCurrentEmployeeIdAsync()))
        {
            return Forbid();
        }

        return Ok(response);
    }

    /// <summary>
    /// Creates a task in a project managed by the current user.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await CanManageProjectAsync(request.ProjectId, cancellationToken))
            {
                return Forbid();
            }

            var response = await _taskService.CreateAsync(request, cancellationToken);

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
    /// Updates a task managed by the current user.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentTask = await _taskService.GetByIdAsync(id, cancellationToken);

            if (currentTask is null)
            {
                return NotFound();
            }

            if (!CanManageTask(currentTask, await GetCurrentEmployeeIdAsync())
                || !await CanManageProjectAsync(request.ProjectId, cancellationToken))
            {
                return Forbid();
            }

            var response = await _taskService.UpdateAsync(id, request, cancellationToken);

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
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Changes a task status according to access and transition rules.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskResponse>> ChangeStatus(
        int id,
        ChangeTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentTask = await _taskService.GetByIdAsync(id, cancellationToken);

            if (currentTask is null)
            {
                return NotFound();
            }

            if (!CanChangeTaskStatus(currentTask, await GetCurrentEmployeeIdAsync()))
            {
                return Forbid();
            }

            var response = await _taskService.ChangeStatusAsync(id, request, cancellationToken);

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
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Deletes a task when deletion is allowed by its status and user permissions.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var currentTask = await _taskService.GetByIdAsync(id, cancellationToken);

            if (currentTask is null)
            {
                return NotFound();
            }

            if (!CanManageTask(currentTask, await GetCurrentEmployeeIdAsync()))
            {
                return Forbid();
            }

            await _taskService.DeleteAsync(id, cancellationToken);

            return NoContent();
        }
        catch (NotFoundException exception)
        {
            return NotFound(new ErrorResponse(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    private IReadOnlyCollection<TaskResponse> FilterTasksByAccess(
        IReadOnlyCollection<TaskResponse> tasks,
        int? employeeId)
    {
        if (User.IsInRole(UserRoles.Head))
        {
            return tasks;
        }

        if (employeeId is null)
        {
            return Array.Empty<TaskResponse>();
        }

        return tasks
            .Where(task => CanViewTask(task, employeeId))
            .ToArray();
    }

    private bool CanViewTask(TaskResponse task, int? employeeId)
    {
        if (User.IsInRole(UserRoles.Head))
        {
            return true;
        }

        if (employeeId is null)
        {
            return false;
        }

        if (User.IsInRole(UserRoles.ProjectManager) && task.Project?.ProjectManagerId == employeeId)
        {
            return true;
        }

        return task.TaskOwnerId == employeeId || task.TaskPerformerId == employeeId;
    }

    private bool CanManageTask(TaskResponse task, int? employeeId)
    {
        return User.IsInRole(UserRoles.Head)
            || (User.IsInRole(UserRoles.ProjectManager)
                && employeeId is not null
                && task.Project?.ProjectManagerId == employeeId);
    }

    private bool CanChangeTaskStatus(TaskResponse task, int? employeeId)
    {
        return CanManageTask(task, employeeId)
            || (User.IsInRole(UserRoles.Employee)
                && employeeId is not null
                && (task.TaskOwnerId == employeeId || task.TaskPerformerId == employeeId));
    }

    private async Task<bool> CanManageProjectAsync(
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

    private static IReadOnlyCollection<TaskResponse> ApplyTaskQuery(
        IReadOnlyCollection<TaskResponse> tasks,
        TaskQueryParameters query)
    {
        IEnumerable<TaskResponse> result = tasks;

        if (query.Status is not null)
        {
            result = result.Where(task => (int)task.TaskStatus == query.Status);
        }

        if (query.ProjectId is not null)
        {
            result = result.Where(task => task.ProjectId == query.ProjectId);
        }

        if (query.PerformerId is not null)
        {
            result = result.Where(task => task.TaskPerformerId == query.PerformerId);
        }

        if (query.OwnerId is not null)
        {
            result = result.Where(task => task.TaskOwnerId == query.OwnerId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();

            result = result.Where(task =>
                $"{task.Name} {task.Project?.Name} {task.TaskOwner?.LastName} {task.TaskOwner?.Name} {task.TaskPerformer?.LastName} {task.TaskPerformer?.Name}"
                    .ToLowerInvariant()
                    .Contains(search));
        }

        result = SortTasks(result, query.SortBy, query.SortDirection);

        return result.ToArray();
    }

    private static IEnumerable<TaskResponse> SortTasks(
        IEnumerable<TaskResponse> tasks,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? tasks.OrderByDescending(x => x.Name) : tasks.OrderBy(x => x.Name),
            "project" => descending ? tasks.OrderByDescending(x => x.Project?.Name) : tasks.OrderBy(x => x.Project?.Name),
            "owner" => descending ? tasks.OrderByDescending(x => x.TaskOwner?.LastName) : tasks.OrderBy(x => x.TaskOwner?.LastName),
            "performer" => descending ? tasks.OrderByDescending(x => x.TaskPerformer?.LastName) : tasks.OrderBy(x => x.TaskPerformer?.LastName),
            "status" => descending ? tasks.OrderByDescending(x => x.TaskStatus) : tasks.OrderBy(x => x.TaskStatus),
            "priority" => descending ? tasks.OrderByDescending(x => x.TaskPriority) : tasks.OrderBy(x => x.TaskPriority),
            _ => tasks.OrderBy(x => x.Id)
        };
    }
}
