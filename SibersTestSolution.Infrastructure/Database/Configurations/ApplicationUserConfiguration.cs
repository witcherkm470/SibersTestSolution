using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SibersTestSolution.Infrastructure.Identity;

namespace SibersTestSolution.Infrastructure.Database.Configurations;

/// <summary>
/// Configures the Identity user mapping.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasOne(x => x.Employee)
            .WithOne()
            .HasForeignKey<ApplicationUser>(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.EmployeeId)
            .IsUnique();
    }
}
