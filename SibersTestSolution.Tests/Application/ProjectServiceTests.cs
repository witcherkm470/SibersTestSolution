using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Projects;
using SibersTestSolution.Tests.Support;
using Employee = SibersTestSolution.Domain.Entities.Employee;
using Project = SibersTestSolution.Domain.Entities.Project;

namespace SibersTestSolution.Tests.Application;

public sealed class ProjectServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesProjectWithManagerAndDistinctEmployees()
    {
        var manager = CreateEmployee(1, "manager@test.local");
        var firstEmployee = CreateEmployee(2, "first@test.local");
        var secondEmployee = CreateEmployee(3, "second@test.local");
        var projectRepository = new FakeProjectRepository();
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            projectRepository,
            new FakeEmployeeRepository(manager, firstEmployee, secondEmployee),
            unitOfWorkManager);

        var response = await service.CreateAsync(new CreateProjectRequest(
            " Project ",
            " Customer ",
            " Contractor ",
            new DateTime(2026, 7, 1),
            new DateTime(2026, 8, 1),
            10,
            manager.Id,
            new[] { firstEmployee.Id, firstEmployee.Id, secondEmployee.Id }));

        Assert.True(projectRepository.AddCalled);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
        Assert.Equal("Project", response.Name);
        Assert.Equal(manager.Id, response.ProjectManagerId);
        Assert.Equal(2, response.Employees.Count);
    }

    [Fact]
    public async Task CreateAsync_AllowsProjectWithoutManagerAndEmployees()
    {
        var projectRepository = new FakeProjectRepository();
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(projectRepository, new FakeEmployeeRepository(), unitOfWorkManager);

        var response = await service.CreateAsync(new CreateProjectRequest(
            "Project",
            "Customer",
            "Contractor",
            new DateTime(2026, 7, 1),
            new DateTime(2026, 8, 1),
            1,
            null,
            null));

        Assert.Null(response.ProjectManagerId);
        Assert.Empty(response.Employees);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
    }

    [Fact]
    public async Task UpdateAsync_ChangesMainProjectDataAndCommits()
    {
        var project = CreateProject(1);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(new FakeProjectRepository(project), new FakeEmployeeRepository(), unitOfWorkManager);

        var response = await service.UpdateAsync(
            project.Id,
            new UpdateProjectRequest(
                "Updated",
                "New customer",
                "New contractor",
                new DateTime(2026, 9, 1),
                new DateTime(2026, 10, 1),
                5));

        Assert.Equal("Updated", response.Name);
        Assert.Equal("New customer", project.CustomerCompanyName);
        Assert.Equal(new DateTime(2026, 9, 1), project.ProjectStartDate);
        Assert.Equal(5, project.ProjectPriority);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task AssignProjectManagerAsync_AssignsActiveEmployeeAndCommits()
    {
        var project = CreateProject(1);
        var manager = CreateEmployee(2, "manager@test.local");
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(manager),
            unitOfWorkManager);

        var response = await service.AssignProjectManagerAsync(
            project.Id,
            new AssignProjectManagerRequest(manager.Id));

        Assert.Equal(manager.Id, response.ProjectManagerId);
        Assert.Same(manager, project.ProjectManager);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task AssignProjectManagerAsync_RejectsDeletedEmployeeAndRollsBack()
    {
        var project = CreateProject(1);
        var manager = CreateEmployee(2, "deleted-manager@test.local");
        manager.MarkDeleted();

        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(manager),
            unitOfWorkManager);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AssignProjectManagerAsync(project.Id, new AssignProjectManagerRequest(manager.Id)));

        Assert.Null(project.ProjectManagerId);
        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task AddEmployeeAsync_AddsActiveEmployeeAndCommits()
    {
        var project = CreateProject(1);
        var employee = CreateEmployee(2, "employee@test.local");
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(employee),
            unitOfWorkManager);

        var response = await service.AddEmployeeAsync(project.Id, new AddProjectEmployeeRequest(employee.Id));

        Assert.Single(response.Employees);
        Assert.Contains(project.Employees, x => x.Id == employee.Id);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task AddEmployeeAsync_RejectsDeletedEmployeeAndRollsBack()
    {
        var project = CreateProject(1);
        var employee = CreateEmployee(2, "deleted@test.local");
        employee.MarkDeleted();

        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(employee),
            unitOfWorkManager);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddEmployeeAsync(project.Id, new AddProjectEmployeeRequest(employee.Id)));

        Assert.Empty(project.Employees);
        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task RemoveEmployeeAsync_RemovesAssignedEmployeeAndCommits()
    {
        var project = CreateProject(1);
        var employee = CreateEmployee(2, "employee@test.local");
        project.AddEmployee(employee);

        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(employee),
            unitOfWorkManager);

        var response = await service.RemoveEmployeeAsync(project.Id, employee.Id);

        Assert.Empty(response.Employees);
        Assert.Empty(project.Employees);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProjectAndCommits()
    {
        var project = CreateProject(1);
        var projectRepository = new FakeProjectRepository(project);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(projectRepository, new FakeEmployeeRepository(), unitOfWorkManager);

        await service.DeleteAsync(project.Id);

        Assert.True(projectRepository.RemoveCalled);
        Assert.Empty(projectRepository.Projects);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    private static IProjectService CreateService(
        FakeProjectRepository projectRepository,
        FakeEmployeeRepository employeeRepository,
        FakeUnitOfWorkManager unitOfWorkManager)
    {
        var serviceType = typeof(IProjectService).Assembly.GetType("SibersTestSolution.Application.Services.ProjectService")
            ?? throw new InvalidOperationException("ProjectService type was not found.");

        var service = Activator.CreateInstance(
            serviceType,
            projectRepository,
            employeeRepository,
            unitOfWorkManager);

        return (IProjectService)(service ?? throw new InvalidOperationException("ProjectService was not created."));
    }

    private static Project CreateProject(int id)
    {
        return new Project(
            "Project",
            "Customer",
            "Contractor",
            new DateTime(2026, 7, 1),
            new DateTime(2026, 8, 1),
            1)
        {
            Id = id
        };
    }

    private static Employee CreateEmployee(int id, string email)
    {
        return new Employee("Ivan", "Ivanov", "Ivanovich", email)
        {
            Id = id
        };
    }
}
