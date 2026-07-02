using FluentValidation;
using SibersTestSolution.Application.DTOs.Projects;

namespace SibersTestSolution.Application.Validation.Projects;

/// <summary>
/// Validates project creation requests.
/// </summary>
public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    /// <summary>
    /// Creates validation rules for project creation.
    /// </summary>
    public CreateProjectRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Project name cannot be empty.");

        RuleFor(request => request.CustomerCompanyName)
            .NotEmpty()
            .WithMessage("Customer company name cannot be empty.");

        RuleFor(request => request.ContractorCompanyName)
            .NotEmpty()
            .WithMessage("Contractor company name cannot be empty.");

        RuleFor(request => request.ProjectStartDate)
            .LessThanOrEqualTo(request => request.ProjectEndDate)
            .WithMessage("Project start date cannot be later than end date.");

        RuleFor(request => request.ProjectPriority)
            .GreaterThan(0)
            .WithMessage("Project priority must be greater than 0.");

        RuleFor(request => request.ProjectManagerId)
            .GreaterThan(0)
            .When(request => request.ProjectManagerId is not null)
            .WithMessage("Project manager id must be greater than 0.");

        RuleForEach(request => request.EmployeeIds)
            .GreaterThan(0)
            .WithMessage("Employee id must be greater than 0.");
    }
}
