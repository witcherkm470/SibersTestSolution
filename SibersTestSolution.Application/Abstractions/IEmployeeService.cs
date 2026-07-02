using SibersTestSolution.Application.DTOs.Employees;

namespace SibersTestSolution.Application.Abstractions;

/// <summary>
/// Provides employee use cases for controllers and other presentation layers.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Gets all employees.
    /// </summary>
    Task<IReadOnlyCollection<EmployeeResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an employee by identifier.
    /// </summary>
    Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates employee data.
    /// </summary>
    Task<EmployeeResponse> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an employee by identifier.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Physically deletes an employee when a freshly created employee must be rolled back.
    /// </summary>
    Task HardDeleteAsync(int id, CancellationToken cancellationToken = default);
}
