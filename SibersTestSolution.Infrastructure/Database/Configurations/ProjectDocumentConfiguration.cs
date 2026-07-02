using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Infrastructure.Database.Configurations;

/// <summary>
/// Configures the project document entity mapping.
/// </summary>
public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("ProjectDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Project)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
