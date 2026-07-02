using FluentValidation;
using SibersTestSolution.Application.DTOs.Projects;

namespace SibersTestSolution.Application.Validation.Projects;

/// <summary>
/// Validates project manager assignment requests.
/// </summary>
public sealed class AssignProjectManagerRequestValidator : AbstractValidator<AssignProjectManagerRequest>
{
    /// <summary>
    /// Creates validation rules for project manager assignment.
    /// </summary>
    public AssignProjectManagerRequestValidator()
    {
        RuleFor(request => request.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Project manager id must be greater than 0.");
    }
}
