using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SibersTestSolution.Api.DTOs;
using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Employees;
using SibersTestSolution.Domain.Constants;
using SibersTestSolution.Infrastructure.Identity;

namespace SibersTestSolution.Api.Controllers;

/// <summary>
/// Provides employee management endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Creates an employees controller.
    /// </summary>
    public EmployeesController(
        IEmployeeService employeeService,
        UserManager<ApplicationUser> userManager)
    {
        _employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Gets employees with optional filtering, sorting, and role data.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeWithRolesResponse>>> GetAll(
        [FromQuery] EmployeeQueryParameters query,
        CancellationToken cancellationToken)
    {
        if (!User.IsInRole(UserRoles.Head) && !User.IsInRole(UserRoles.ProjectManager))
        {
            return Forbid();
        }

        var employees = ApplyEmployeeQuery(await _employeeService.GetAllAsync(cancellationToken), query);
        var response = await AddRolesAsync(employees);

        return Ok(response);
    }

    /// <summary>
    /// Gets an employee by identifier.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeWithRolesResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _employeeService.GetByIdAsync(id, cancellationToken);

        if (response is null)
        {
            return NotFound();
        }

        return Ok(await AddRolesAsync(response));
    }

    /// <summary>
    /// Creates an employee and optionally issues login credentials.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpPost]
    public async Task<ActionResult<EmployeeWithRolesResponse>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _employeeService.CreateAsync(request, cancellationToken);

            try
            {
                await CreateOrUpdateIdentityUserIfRequestedAsync(
                    request.Email,
                    request.UserName,
                    request.Password,
                    request.Role,
                    response.Id);
            }
            catch
            {
                await _employeeService.HardDeleteAsync(response.Id, cancellationToken);
                throw;
            }

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, await AddRolesAsync(response));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ErrorResponse(exception.Message));
        }
    }

    /// <summary>
    /// Updates an employee and optionally updates login credentials.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EmployeeWithRolesResponse>> Update(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _employeeService.UpdateAsync(id, request, cancellationToken);
            await CreateOrUpdateIdentityUserIfRequestedAsync(
                request.Email,
                request.UserName,
                request.Password,
                request.Role,
                response.Id);

            return Ok(await AddRolesAsync(response));
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
    /// Deletes an employee by identifier.
    /// </summary>
    [Authorize(Roles = UserRoles.Head)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeService.GetByIdAsync(id, cancellationToken);

            if (employee is null)
            {
                return NotFound(new ErrorResponse($"Employee with id '{id}' was not found."));
            }

            await DeleteIdentityUserIfExistsAsync(id, cancellationToken);
            await _employeeService.DeleteAsync(id, cancellationToken);

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

    private async Task CreateOrUpdateIdentityUserIfRequestedAsync(
        string email,
        string? userName,
        string? password,
        string? role,
        int employeeId)
    {
        if (string.IsNullOrWhiteSpace(userName)
            && string.IsNullOrWhiteSpace(password)
            && string.IsNullOrWhiteSpace(role))
        {
            return;
        }

        var normalizedUserName = userName!.Trim();
        var normalizedEmail = email.Trim();
        var normalizedPassword = string.IsNullOrWhiteSpace(password) ? null : password.Trim();
        var normalizedRole = role!.Trim();

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
        var userByName = await _userManager.FindByNameAsync(normalizedUserName);
        var createdUser = false;

        if (userByName is not null && userByName.EmployeeId != employeeId)
        {
            throw new InvalidOperationException($"User name '{normalizedUserName}' is already used.");
        }

        if (user is null)
        {
            if (normalizedPassword is null)
            {
                throw new InvalidOperationException("Password is required when employee account is created.");
            }

            user = new ApplicationUser
            {
                UserName = normalizedUserName,
                Email = normalizedEmail,
                EmailConfirmed = true,
                EmployeeId = employeeId
            };

            var createResult = await _userManager.CreateAsync(user, normalizedPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(ToIdentityError("Failed to create user", createResult));
            }

            createdUser = true;
        }
        else
        {
            user.UserName = normalizedUserName;
            user.Email = normalizedEmail;
            user.EmailConfirmed = true;
            user.EmployeeId = employeeId;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(ToIdentityError("Failed to update user", updateResult));
            }

            if (normalizedPassword is not null)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetToken, normalizedPassword);

                if (!resetPasswordResult.Succeeded)
                {
                    throw new InvalidOperationException(ToIdentityError("Failed to reset user password", resetPasswordResult));
                }
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Count > 0)
        {
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeRolesResult.Succeeded)
            {
                throw new InvalidOperationException(ToIdentityError("Failed to update user role", removeRolesResult));
            }
        }

        var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);

        if (!roleResult.Succeeded)
        {
            if (createdUser)
            {
                await _userManager.DeleteAsync(user);
            }

            throw new InvalidOperationException(ToIdentityError("Failed to assign user role", roleResult));
        }
    }

    private static string ToIdentityError(string prefix, IdentityResult result)
    {
        return $"{prefix}: {string.Join("; ", result.Errors.Select(x => x.Description))}";
    }

    private async Task DeleteIdentityUserIfExistsAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(
            x => x.EmployeeId == employeeId,
            cancellationToken);

        if (user is null)
        {
            return;
        }

        var deleteResult = await _userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            throw new InvalidOperationException(ToIdentityError("Failed to delete user account", deleteResult));
        }
    }

    private static IReadOnlyCollection<EmployeeResponse> ApplyEmployeeQuery(
        IReadOnlyCollection<EmployeeResponse> employees,
        EmployeeQueryParameters query)
    {
        IEnumerable<EmployeeResponse> result = employees;

        var queryText = query.Search ?? query.Query;

        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var search = queryText.Trim().ToLowerInvariant();

            result = result.Where(employee =>
                $"{employee.LastName} {employee.Name} {employee.MiddleName} {employee.Email}"
                    .ToLowerInvariant()
                    .Contains(search));
        }

        result = SortEmployees(result, query.SortBy, query.SortDirection);

        return result.ToArray();
    }

    private static IEnumerable<EmployeeResponse> SortEmployees(
        IEnumerable<EmployeeResponse> employees,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? employees.OrderByDescending(x => x.Name) : employees.OrderBy(x => x.Name),
            "lastname" => descending ? employees.OrderByDescending(x => x.LastName) : employees.OrderBy(x => x.LastName),
            "middlename" => descending ? employees.OrderByDescending(x => x.MiddleName) : employees.OrderBy(x => x.MiddleName),
            "email" => descending ? employees.OrderByDescending(x => x.Email) : employees.OrderBy(x => x.Email),
            _ => employees.OrderBy(x => x.Id)
        };
    }

    private async Task<IReadOnlyCollection<EmployeeWithRolesResponse>> AddRolesAsync(
        IReadOnlyCollection<EmployeeResponse> employees)
    {
        var result = new List<EmployeeWithRolesResponse>(employees.Count);

        foreach (var employee in employees)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id);
            var roles = user is null
                ? Array.Empty<string>()
                : (await _userManager.GetRolesAsync(user)).ToArray();

            result.Add(EmployeeWithRolesResponse.From(employee, user?.UserName, roles));
        }

        return result;
    }

    private async Task<EmployeeWithRolesResponse> AddRolesAsync(EmployeeResponse employee)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id);
        var roles = user is null
            ? Array.Empty<string>()
            : (await _userManager.GetRolesAsync(user)).ToArray();

        return EmployeeWithRolesResponse.From(employee, user?.UserName, roles);
    }
}
