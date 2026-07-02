using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Infrastructure.Database.Configurations;

/// <summary>
/// Configures the employee entity mapping.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MiddleName).HasMaxLength(100);
        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = 0");
    }
}
