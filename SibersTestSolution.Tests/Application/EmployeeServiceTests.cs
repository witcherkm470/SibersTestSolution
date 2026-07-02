using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Employees;
using SibersTestSolution.Tests.Support;
using Employee = SibersTestSolution.Domain.Entities.Employee;

namespace SibersTestSolution.Tests.Application;

public sealed class EmployeeServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveEmployees()
    {
        var activeEmployee = CreateEmployee(1, "active@test.local");
        var deletedEmployee = CreateEmployee(2, "deleted@test.local");
        deletedEmployee.MarkDeleted();

        var service = CreateService(new FakeEmployeeRepository(activeEmployee, deletedEmployee), new FakeUnitOfWorkManager());

        var employees = await service.GetAllAsync();

        var employee = Assert.Single(employees);
        Assert.Equal(activeEmployee.Id, employee.Id);
        Assert.False(employee.IsDeleted);
    }

    [Fact]
    public async Task CreateAsync_AddsEmployeeAndCommits()
    {
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(employeeRepository, unitOfWorkManager);

        var response = await service.CreateAsync(new CreateEmployeeRequest(
            " Ivan ",
            " Ivanov ",
            " Ivanovich ",
            " ivan@test.local "));

        Assert.True(employeeRepository.AddCalled);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
        Assert.Equal("Ivan", response.Name);
        Assert.Equal("ivan@test.local", response.Email);
        Assert.False(response.IsDeleted);
    }

    [Fact]
    public async Task UpdateAsync_ChangesActiveEmployeeAndCommits()
    {
        var employee = CreateEmployee(1, "old@test.local");
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(new FakeEmployeeRepository(employee), unitOfWorkManager);

        var response = await service.UpdateAsync(
            employee.Id,
            new UpdateEmployeeRequest("Petr", "Petrov", "Petrovich", "petr@test.local"));

        Assert.Equal("Petr", response.Name);
        Assert.Equal("Petrov", employee.LastName);
        Assert.Equal("petr@test.local", employee.Email);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task UpdateAsync_RejectsDeletedEmployeeAndRollsBack()
    {
        var employee = CreateEmployee(1, "deleted@test.local");
        employee.MarkDeleted();

        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(new FakeEmployeeRepository(employee), unitOfWorkManager);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(
                employee.Id,
                new UpdateEmployeeRequest("Petr", "Petrov", "Petrovich", "petr@test.local")));

        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task DeleteAsync_MarksEmployeeDeletedAndCommitsWithoutHardRemove()
    {
        var employee = CreateEmployee(1, "employee@test.local");
        var employeeRepository = new FakeEmployeeRepository(employee);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(employeeRepository, unitOfWorkManager);

        await service.DeleteAsync(employee.Id);

        Assert.True(employee.IsDeleted);
        Assert.False(employeeRepository.RemoveCalled);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task HardDeleteAsync_RemovesEmployeeAndCommits()
    {
        var employee = CreateEmployee(1, "employee@test.local");
        var employeeRepository = new FakeEmployeeRepository(employee);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(employeeRepository, unitOfWorkManager);

        await service.HardDeleteAsync(employee.Id);

        Assert.True(employeeRepository.RemoveCalled);
        Assert.Empty(employeeRepository.Employees);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    private static IEmployeeService CreateService(
        FakeEmployeeRepository employeeRepository,
        FakeUnitOfWorkManager unitOfWorkManager)
    {
        var serviceType = typeof(IEmployeeService).Assembly.GetType("SibersTestSolution.Application.Services.EmployeeService")
            ?? throw new InvalidOperationException("EmployeeService type was not found.");

        var service = Activator.CreateInstance(serviceType, employeeRepository, unitOfWorkManager);

        return (IEmployeeService)(service ?? throw new InvalidOperationException("EmployeeService was not created."));
    }

    private static Employee CreateEmployee(int id, string email)
    {
        return new Employee("Ivan", "Ivanov", "Ivanovich", email)
        {
            Id = id
        };
    }
}
