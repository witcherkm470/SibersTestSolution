using FluentValidation;
using SibersTestSolution.Api.DTOs;

namespace SibersTestSolution.Api.Validation;

/// <summary>
/// Validates project query parameters.
/// </summary>
public sealed class ProjectQueryParametersValidator : AbstractValidator<ProjectQueryParameters>
{
    private static readonly string[] SortFields =
    [
        "name",
        "customer",
        "contractor",
        "startdate",
        "enddate",
        "priority"
    ];

    /// <summary>
    /// Creates validation rules for project filtering and sorting.
    /// </summary>
    public ProjectQueryParametersValidator()
    {
        RuleFor(query => query.StartDateFrom)
            .LessThanOrEqualTo(query => query.StartDateTo)
            .When(query => query.StartDateFrom is not null && query.StartDateTo is not null)
            .WithMessage("Start date from cannot be later than start date to.");

        RuleFor(query => query.Priority)
            .GreaterThan(0)
            .When(query => query.Priority is not null)
            .WithMessage("Priority must be greater than 0.");

        RuleFor(query => query.SortBy)
            .Must(value => string.IsNullOrWhiteSpace(value) || SortFields.Contains(value.Trim().ToLowerInvariant()))
            .WithMessage("Project sort field has invalid value.");

        RuleFor(query => query.SortDirection)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}
