using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Employees;
using SibersTestSolution.Infrastructure.Abstractions;
using SibersTestSolution.Infrastructure.Repositories.Abstractions;
using Employee = SibersTestSolution.Domain.Entities.Employee;

namespace SibersTestSolution.Application.Services;

/// <summary>
/// Implements employee management use cases.
/// </summary>
internal sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    /// <summary>
    /// Creates an employee service.
    /// </summary>
    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _unitOfWorkManager = unitOfWorkManager ?? throw new ArgumentNullException(nameof(unitOfWorkManager));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<EmployeeResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        return employees
            .Where(employee => !employee.IsDeleted)
            .Select(EmployeeResponse.From)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        return employee is null || employee.IsDeleted ? null : EmployeeResponse.From(employee);
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();
        var employee = new Employee(
            request.Name,
            request.LastName,
            request.MiddleName,
            request.Email);

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return EmployeeResponse.From(employee);
    }

    /// <inheritdoc />
    public async Task<EmployeeResponse> UpdateAsync(
        int id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var unitOfWork = _unitOfWorkManager.Create();
        var employee = await GetEmployeeOrThrowAsync(id, cancellationToken);

        employee.ChangeFullName(request.Name, request.LastName, request.MiddleName);
        employee.ChangeEmail(request.Email);

        await unitOfWork.CommitAsync(cancellationToken);

        return EmployeeResponse.From(employee);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkManager.Create();
        var employee = await GetEmployeeOrThrowAsync(id, cancellationToken);

        employee.MarkDeleted();
        await unitOfWork.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var unitOfWork = _unitOfWorkManager.Create();
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException($"Employee with id '{id}' was not found.");
        }

        _employeeRepository.Remove(employee);
        await unitOfWork.CommitAsync(cancellationToken);
    }

    private async Task<Employee> GetEmployeeOrThrowAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);

        if (employee is null || employee.IsDeleted)
        {
            throw new NotFoundException($"Employee with id '{id}' was not found.");
        }

        return employee;
    }
}
