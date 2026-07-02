namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Response containing the current authenticated user data.
/// </summary>
public sealed record CurrentUserResponse(
    string Id,
    string UserName,
    string? Email,
    int? EmployeeId,
    IReadOnlyCollection<string> Roles);
