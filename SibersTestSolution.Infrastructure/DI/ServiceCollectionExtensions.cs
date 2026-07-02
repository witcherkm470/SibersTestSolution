using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SibersTestSolution.Infrastructure.Abstractions;
using SibersTestSolution.Infrastructure.Database;
using SibersTestSolution.Infrastructure.Internal;
using SibersTestSolution.Infrastructure.Options;
using SibersTestSolution.Infrastructure.Repositories;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;

namespace SibersTestSolution.Infrastructure.DI;

/// <summary>
/// Registers infrastructure services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Name of the configured database connection string.
    /// </summary>
    public const string ConnectionStringName = "SibersTestSolutionDb";

    /// <summary>
    /// Adds database, options, repositories, and unit-of-work services.
    /// </summary>
    public static IServiceCollection AddSibersTestSolutionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);

        services.AddAdminUserOptions(configuration);
        services.AddProjectDocumentOptions(configuration);
        
        services.AddDatabase(connectionString);
        services.AddRepositories();

        return services;
    }

    public static IServiceCollection AddAdminUserOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AdminUserOptions>(options =>
        {
            var section = configuration.GetSection(AdminUserOptions.SectionName);
            options.UserName = section[nameof(AdminUserOptions.UserName)] ?? options.UserName;
            options.Password = section[nameof(AdminUserOptions.Password)] ?? options.Password;
            options.Email = section[nameof(AdminUserOptions.Email)] ?? options.Email;
        });
        
        return services;
    }
    
    public static IServiceCollection AddProjectDocumentOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProjectDocumentOptions>(options =>
        {
            var section = configuration.GetSection(ProjectDocumentOptions.SectionName);
            options.StoragePath = section[nameof(ProjectDocumentOptions.StoragePath)] ?? options.StoragePath;

            if (long.TryParse(section[nameof(ProjectDocumentOptions.MaxFileSizeBytes)], out var maxFileSizeBytes))
            {
                options.MaxFileSizeBytes = maxFileSizeBytes;
            }
        });
        
        return services;
    }

    /// <summary>
    /// Adds the SQLite database context and unit-of-work services.
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Database connection string cannot be empty.", nameof(connectionString));
        }

        services.AddDbContext<SibersTestSolutionDbContext>(options => options.UseSqlite(connectionString));
        services.AddUnitOfWork<SibersTestSolutionDbContext>();

        return services;
    }

    /// <summary>
    /// Adds repository implementations.
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }

    /// <summary>
    /// Adds unit-of-work services for the specified database context.
    /// </summary>
    public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IUnitOfWorkDbContext, UnitOfWorkDbContext<TDbContext>>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
        services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");
        }

        return connectionString;
    }
    
    
}
