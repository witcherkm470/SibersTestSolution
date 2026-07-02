using FluentValidation;
using SibersTestSolution.Application.DTOs.Tasks;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Application.Validation.Tasks;

/// <summary>
/// Validates task status change requests.
/// </summary>
public sealed class ChangeTaskStatusRequestValidator : AbstractValidator<ChangeTaskStatusRequest>
{
    /// <summary>
    /// Creates validation rules for task status changes.
    /// </summary>
    public ChangeTaskStatusRequestValidator()
    {
        RuleFor(request => request.TaskStatus)
            .Must(status => Enum.IsDefined(status) && status != TaskStatus.Default)
            .WithMessage("Task status has invalid value.");
    }
}
