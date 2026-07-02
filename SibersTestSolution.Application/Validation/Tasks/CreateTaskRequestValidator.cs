using FluentValidation;
using SibersTestSolution.Application.DTOs.Tasks;

namespace SibersTestSolution.Application.Validation.Tasks;

/// <summary>
/// Validates task creation requests.
/// </summary>
public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    /// <summary>
    /// Creates validation rules for task creation.
    /// </summary>
    public CreateTaskRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Task name cannot be empty.");

        RuleFor(request => request.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project id must be greater than 0.");

        RuleFor(request => request.TaskOwnerId)
            .GreaterThan(0)
            .WithMessage("Task owner id must be greater than 0.");

        RuleFor(request => request.TaskPerformerId)
            .GreaterThan(0)
            .When(request => request.TaskPerformerId is not null)
            .WithMessage("Task performer id must be greater than 0.");

        RuleFor(request => request.TaskPriority)
            .GreaterThan(0)
            .WithMessage("Task priority must be greater than 0.");
    }
}
