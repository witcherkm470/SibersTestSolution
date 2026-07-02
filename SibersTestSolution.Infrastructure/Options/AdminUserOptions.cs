namespace SibersTestSolution.Infrastructure.Options;

/// <summary>
/// Configuration for the seeded administrator account.
/// </summary>
public class AdminUserOptions
{
    /// <summary>
    /// Gets the configuration section name.
    /// </summary>
    public const string SectionName = "AdminUser";

    /// <summary>
    /// Gets or sets the administrator login.
    /// </summary>
    public string UserName { get; set; } = "admin";

    /// <summary>
    /// Gets or sets the administrator password.
    /// </summary>
    public string Password { get; set; } = "admin";

    /// <summary>
    /// Gets or sets the administrator email.
    /// </summary>
    public string Email { get; set; } = "admin@sibers.local";
}
