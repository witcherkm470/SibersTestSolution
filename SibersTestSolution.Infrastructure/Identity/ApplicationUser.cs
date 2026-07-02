using Microsoft.AspNetCore.Identity;
using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Infrastructure.Identity;

/// <summary>
/// Identity user linked to an optional employee profile.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the linked employee identifier.
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the linked employee navigation property.
    /// </summary>
    public Employee? Employee { get; set; }
}
