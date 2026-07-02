using FluentValidation;
using SibersTestSolution.Application.DTOs.Tasks;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Application.Validation.Tasks;

/// <summary>
/// Validates task update requests.
/// </summary>
public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    /// <summary>
    /// Creates validation rules for task updates.
    /// </summary>
    public UpdateTaskRequestValidator()
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

        RuleFor(request => request.TaskStatus)
            .Must(status => Enum.IsDefined(status) && status != TaskStatus.Default)
            .WithMessage("Task status has invalid value.");

        RuleFor(request => request.TaskPriority)
            .GreaterThan(0)
            .WithMessage("Task priority must be greater than 0.");
    }
}
