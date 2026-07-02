using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task = SibersTestSolution.Domain.Entities.Task;

namespace SibersTestSolution.Infrastructure.Database.Configurations;

/// <summary>
/// Configures the task entity mapping.
/// </summary>
public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(5000);

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Tasks)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TaskOwner)
            .WithMany(x => x.CreatedTasks)
            .HasForeignKey(x => x.TaskOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TaskPerformer)
            .WithMany(x => x.AssignedTasks)
            .HasForeignKey(x => x.TaskPerformerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
