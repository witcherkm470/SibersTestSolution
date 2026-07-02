using FluentValidation;
using SibersTestSolution.Application.DTOs.Employees;

namespace SibersTestSolution.Application.Validation.Employees;

/// <summary>
/// Validates employee update requests.
/// </summary>
public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    /// <summary>
    /// Creates validation rules for employee updates.
    /// </summary>
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(request => request.LastName)
            .NotEmpty()
            .WithMessage("Employee last name cannot be empty.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Employee name cannot be empty.");

        RuleFor(request => request.MiddleName)
            .NotEmpty()
            .WithMessage("Employee middle name cannot be empty.");

        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Employee email cannot be empty.")
            .EmailAddress()
            .WithMessage("Employee email has invalid format.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            .WithMessage("Employee email has invalid format.");

        RuleFor(request => request.UserName)
            .NotEmpty()
            .When(request => HasAnyLoginField(request))
            .WithMessage("Login is required when employee account is created or updated.")
            .Matches(@"^[A-Za-z0-9._@+-]+$")
            .When(request => !string.IsNullOrWhiteSpace(request.UserName))
            .WithMessage("Login can contain only latin letters, digits and . _ @ + -.");

        RuleFor(request => request.Password)
            .MinimumLength(3)
            .When(request => !string.IsNullOrWhiteSpace(request.Password))
            .WithMessage("Password must be at least 3 characters long.");

        RuleFor(request => request.Role)
            .NotEmpty()
            .When(request => HasAnyLoginField(request))
            .WithMessage("Role is required when employee account is created or updated.")
            .Must(value => value is null || SibersTestSolution.Domain.Constants.UserRoles.All.Contains(value))
            .WithMessage("User role has invalid value.");
    }

    private static bool HasAnyLoginField(UpdateEmployeeRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.UserName)
            || !string.IsNullOrWhiteSpace(request.Password)
            || !string.IsNullOrWhiteSpace(request.Role);
    }
}
