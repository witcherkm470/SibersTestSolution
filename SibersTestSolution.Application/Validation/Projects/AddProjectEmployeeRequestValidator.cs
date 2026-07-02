using FluentValidation;
using SibersTestSolution.Application.DTOs.Projects;

namespace SibersTestSolution.Application.Validation.Projects;

/// <summary>
/// Validates requests for adding employees to projects.
/// </summary>
public sealed class AddProjectEmployeeRequestValidator : AbstractValidator<AddProjectEmployeeRequest>
{
    /// <summary>
    /// Creates validation rules for adding employees to projects.
    /// </summary>
    public AddProjectEmployeeRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee id must be greater than 0.");
    }
}
