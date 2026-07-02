using FluentValidation;
using SibersTestSolution.Api.DTOs;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Api.Validation;

/// <summary>
/// Validates task query parameters.
/// </summary>
public sealed class TaskQueryParametersValidator : AbstractValidator<TaskQueryParameters>
{
    private static readonly string[] SortFields =
    [
        "name",
        "project",
        "owner",
        "performer",
        "status",
        "priority"
    ];

    /// <summary>
    /// Creates validation rules for task filtering and sorting.
    /// </summary>
    public TaskQueryParametersValidator()
    {
        RuleFor(query => query.Status)
            .Must(status => status is null || Enum.IsDefined(typeof(TaskStatus), status.Value) && status.Value != (int)TaskStatus.Default)
            .WithMessage("Task status has invalid value.");

        RuleFor(query => query.ProjectId)
            .GreaterThan(0)
            .When(query => query.ProjectId is not null)
            .WithMessage("Project id must be greater than 0.");

        RuleFor(query => query.PerformerId)
            .GreaterThan(0)
            .When(query => query.PerformerId is not null)
            .WithMessage("Task performer id must be greater than 0.");

        RuleFor(query => query.OwnerId)
            .GreaterThan(0)
            .When(query => query.OwnerId is not null)
            .WithMessage("Task owner id must be greater than 0.");

        RuleFor(query => query.SortBy)
            .Must(value => string.IsNullOrWhiteSpace(value) || SortFields.Contains(value.Trim().ToLowerInvariant()))
            .WithMessage("Task sort field has invalid value.");

        RuleFor(query => query.SortDirection)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}
