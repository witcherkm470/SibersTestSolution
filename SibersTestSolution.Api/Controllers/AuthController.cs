using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SibersTestSolution.Api.DTOs;
using SibersTestSolution.Infrastructure.Identity;

namespace SibersTestSolution.Api.Controllers;

/// <summary>
/// Provides authentication endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Creates an authentication controller.
    /// </summary>
    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Signs in a user using login and password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.UserName.Trim(),
            request.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized(new ErrorResponse("Invalid login or password."));
        }

        var user = await _userManager.FindByNameAsync(request.UserName.Trim());

        if (user is null)
        {
            return Unauthorized(new ErrorResponse("Invalid login or password."));
        }

        return Ok(await ToCurrentUserResponseAsync(user));
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return NoContent();
    }

    /// <summary>
    /// Gets the current authenticated user.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await ToCurrentUserResponseAsync(user));
    }

    private async Task<CurrentUserResponse> ToCurrentUserResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email,
            user.EmployeeId,
            roles.ToArray());
    }
}
