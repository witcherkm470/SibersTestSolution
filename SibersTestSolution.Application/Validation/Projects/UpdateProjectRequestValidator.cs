using FluentValidation;
using SibersTestSolution.Application.DTOs.Projects;

namespace SibersTestSolution.Application.Validation.Projects;

/// <summary>
/// Validates project update requests.
/// </summary>
public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    /// <summary>
    /// Creates validation rules for project updates.
    /// </summary>
    public UpdateProjectRequestValidator()
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
    }
}
