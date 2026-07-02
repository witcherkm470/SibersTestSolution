using FluentValidation;
using SibersTestSolution.Api.DTOs;

namespace SibersTestSolution.Api.Validation;

/// <summary>
/// Validates employee query parameters.
/// </summary>
public sealed class EmployeeQueryParametersValidator : AbstractValidator<EmployeeQueryParameters>
{
    private static readonly string[] SortFields =
    [
        "name",
        "lastname",
        "middlename",
        "email"
    ];

    /// <summary>
    /// Creates validation rules for employee filtering and sorting.
    /// </summary>
    public EmployeeQueryParametersValidator()
    {
        RuleFor(query => query.SortBy)
            .Must(value => string.IsNullOrWhiteSpace(value) || SortFields.Contains(value.Trim().ToLowerInvariant()))
            .WithMessage("Employee sort field has invalid value.");

        RuleFor(query => query.SortDirection)
            .Must(value => string.IsNullOrWhiteSpace(value)
                || string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sort direction must be 'asc' or 'desc'.");
    }
}
