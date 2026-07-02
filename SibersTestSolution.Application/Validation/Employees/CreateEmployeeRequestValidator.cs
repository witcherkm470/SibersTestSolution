using FluentValidation;
using SibersTestSolution.Application.DTOs.Employees;

namespace SibersTestSolution.Application.Validation.Employees;

/// <summary>
/// Validates employee creation requests.
/// </summary>
public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    /// <summary>
    /// Creates validation rules for employee creation.
    /// </summary>
    public CreateEmployeeRequestValidator()
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

        EmployeeLoginValidationRules.Apply(
            RuleFor(request => request.UserName),
            RuleFor(request => request.Password),
            RuleFor(request => request.Role),
            request => request.UserName,
            request => request.Password,
            request => request.Role);
    }
}
