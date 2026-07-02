using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SibersTestSolution.Domain.Constants;
using SibersTestSolution.Infrastructure.Options;

namespace SibersTestSolution.Infrastructure.Identity;

/// <summary>
/// Seeds Identity roles and the configured administrator account.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>
    /// Ensures that application roles and the head administrator user exist.
    /// </summary>
    public static async Task SeedIdentityAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminUserOptions>>().Value;

        foreach (var role in UserRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var userName = adminOptions.UserName;
        var password = adminOptions.Password;
        var email = adminOptions.Email;

        var admin = await userManager.FindByNameAsync(userName);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(ToErrorMessage("Failed to create admin user", createResult));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Head))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, UserRoles.Head);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(ToErrorMessage("Failed to assign admin role", roleResult));
            }
        }
    }

    private static string ToErrorMessage(string prefix, IdentityResult result)
    {
        return $"{prefix}: {string.Join("; ", result.Errors.Select(x => x.Description))}";
    }
}
