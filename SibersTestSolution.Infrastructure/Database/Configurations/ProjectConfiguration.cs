using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Infrastructure.Database.Configurations;

/// <summary>
/// Configures the project entity mapping.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CustomerCompanyName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ContractorCompanyName).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.ProjectManager)
            .WithMany(x => x.ManagedProjects)
            .HasForeignKey(x => x.ProjectManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Employees)
            .WithMany(x => x.Projects)
            .UsingEntity("ProjectEmployees");

        builder.Navigation(x => x.Employees)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Documents)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
