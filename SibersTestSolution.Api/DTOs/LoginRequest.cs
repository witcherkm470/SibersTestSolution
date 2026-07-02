namespace SibersTestSolution.Api.DTOs;

/// <summary>
/// Request for signing in with login and password.
/// </summary>
public sealed record LoginRequest(string UserName, string Password);
