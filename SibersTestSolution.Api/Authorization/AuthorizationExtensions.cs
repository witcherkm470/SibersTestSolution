using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SibersTestSolution.Domain.Constants;
using SibersTestSolution.Infrastructure.Identity;

namespace SibersTestSolution.Api.Authorization;

/// <summary>
/// Helper methods for reading authorization data from the current user.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Determines whether the user has the head role.
    /// </summary>
    public static bool IsHead(this ClaimsPrincipal user)
    {
        return user.IsInRole(UserRoles.Head);
    }

    /// <summary>
    /// Determines whether the user has the project manager role.
    /// </summary>
    public static bool IsProjectManager(this ClaimsPrincipal user)
    {
        return user.IsInRole(UserRoles.ProjectManager);
    }

    /// <summary>
    /// Determines whether the user has the employee role.
    /// </summary>
    public static bool IsEmployee(this ClaimsPrincipal user)
    {
        return user.IsInRole(UserRoles.Employee);
    }

    /// <summary>
    /// Gets the employee identifier linked to the current Identity user.
    /// </summary>
    public static async Task<int?> GetEmployeeIdAsync(
        this ClaimsPrincipal user,
        UserManager<ApplicationUser> userManager)
    {
        var applicationUser = await userManager.GetUserAsync(user);

        return applicationUser?.EmployeeId;
    }
}
