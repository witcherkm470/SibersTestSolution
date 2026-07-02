using Microsoft.Extensions.DependencyInjection;
using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Services;

namespace SibersTestSolution.Application.DI;

/// <summary>
/// Registers application layer services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application use-case services.
    /// </summary>
    public static IServiceCollection AddSibersTestSolutionApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITaskService, TaskService>();

        return services;
    }
}
