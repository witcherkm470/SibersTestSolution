using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SibersTestSolution.Infrastructure.Database;

/// <summary>
/// Creates the database context for EF Core design-time commands.
/// </summary>
public class SibersTestSolutionDbContextFactory : IDesignTimeDbContextFactory<SibersTestSolutionDbContext>
{
    /// <summary>
    /// Creates a database context configured for the local SQLite database.
    /// </summary>
    public SibersTestSolutionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SibersTestSolutionDbContext>();
        var currentDirectory = Directory.GetCurrentDirectory();
        var databasePath = string.Equals(
            Path.GetFileName(currentDirectory),
            "SibersTestSolution.Api",
            StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(currentDirectory, "sibers-test.db")
            : Path.Combine(currentDirectory, "SibersTestSolution.Api", "sibers-test.db");
        optionsBuilder.UseSqlite($"Data Source={databasePath}");

        return new SibersTestSolutionDbContext(optionsBuilder.Options);
    }
}
