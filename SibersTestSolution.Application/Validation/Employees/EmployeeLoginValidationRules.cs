using FluentValidation;
using SibersTestSolution.Domain.Constants;

namespace SibersTestSolution.Application.Validation.Employees;

/// <summary>
/// Adds shared validation rules for optional employee login credentials.
/// </summary>
internal static class EmployeeLoginValidationRules
{
    private const string LoginPattern = @"^[A-Za-z0-9._@+-]+$";

    /// <summary>
    /// Applies login, password, and role validation rules to an employee request validator.
    /// </summary>
    public static void Apply<T>(
        IRuleBuilderInitial<T, string?> userNameRule,
        IRuleBuilderInitial<T, string?> passwordRule,
        IRuleBuilderInitial<T, string?> roleRule,
        Func<T, string?> userName,
        Func<T, string?> password,
        Func<T, string?> role)
    {
        userNameRule
            .NotEmpty()
            .When(request => HasAnyLoginField(userName(request), password(request), role(request)))
            .WithMessage("Login is required when employee account is created or updated.")
            .Matches(LoginPattern)
            .When(request => !string.IsNullOrWhiteSpace(userName(request)))
            .WithMessage("Login can contain only latin letters, digits and . _ @ + -.");

        passwordRule
            .NotEmpty()
            .When(request => HasAnyLoginField(userName(request), password(request), role(request)))
            .WithMessage("Password is required when employee account is created or updated.")
            .MinimumLength(3)
            .When(request => !string.IsNullOrWhiteSpace(password(request)))
            .WithMessage("Password must be at least 3 characters long.");

        roleRule
            .NotEmpty()
            .When(request => HasAnyLoginField(userName(request), password(request), role(request)))
            .WithMessage("Role is required when employee account is created or updated.")
            .Must(value => value is null || UserRoles.All.Contains(value))
            .WithMessage("User role has invalid value.");
    }

    private static bool HasAnyLoginField(string? userName, string? password, string? role)
    {
        return !string.IsNullOrWhiteSpace(userName)
            || !string.IsNullOrWhiteSpace(password)
            || !string.IsNullOrWhiteSpace(role);
    }
}
