using FluentValidation;
using SibersTestSolution.Api.DTOs;

namespace SibersTestSolution.Api.Validation;

/// <summary>
/// Validates login requests.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// Creates validation rules for login requests.
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(request => request.UserName)
            .NotEmpty()
            .WithMessage("Login is required.");

        RuleFor(request => request.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
