using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SibersTestSolution.Domain.Entities;
using SibersTestSolution.Infrastructure.Identity;
using Task = SibersTestSolution.Domain.Entities.Task;

namespace SibersTestSolution.Infrastructure.Database;

/// <summary>
/// Entity Framework database context for the application and Identity data.
/// </summary>
public class SibersTestSolutionDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Creates the application database context.
    /// </summary>
    public SibersTestSolutionDbContext(DbContextOptions<SibersTestSolutionDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the projects table.
    /// </summary>
    public DbSet<Project> Projects => Set<Project>();

    /// <summary>
    /// Gets the employees table.
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>
    /// Gets the tasks table.
    /// </summary>
    public DbSet<Task> Tasks => Set<Task>();

    /// <summary>
    /// Gets the project documents table.
    /// </summary>
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SibersTestSolutionDbContext).Assembly);
    }
}
