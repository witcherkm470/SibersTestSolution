using SibersTestSolution.Domain.Entities;

namespace SibersTestSolution.Tests.Domain;

public sealed class ProjectTests
{
    [Fact]
    public void Constructor_AllowsProjectWithoutManager()
    {
        var project = CreateProject();

        Assert.Null(project.ProjectManagerId);
        Assert.Null(project.ProjectManager);
    }

    [Fact]
    public void Constructor_RejectsStartDateLaterThanEndDate()
    {
        var startDate = new DateTime(2026, 7, 2);
        var endDate = new DateTime(2026, 7, 1);

        Assert.Throws<ArgumentException>(() => CreateProject(startDate, endDate));
    }

    [Fact]
    public void AddEmployee_DoesNotAddSamePersistedEmployeeTwice()
    {
        var project = CreateProject();
        var firstEmployee = CreateEmployee(1);
        var sameEmployeeFromAnotherLoad = CreateEmployee(1);

        project.AddEmployee(firstEmployee);
        project.AddEmployee(sameEmployeeFromAnotherLoad);

        var employee = Assert.Single(project.Employees);
        Assert.Same(firstEmployee, employee);
    }

    [Fact]
    public void RemoveEmployee_RejectsInvalidEmployeeId()
    {
        var project = CreateProject();

        Assert.Throws<ArgumentException>(() => project.RemoveEmployee(0));
    }

    [Fact]
    public void AssignProjectManager_StoresManagerAndCanRemoveIt()
    {
        var project = CreateProject();
        var manager = CreateEmployee(7);

        project.AssignProjectManager(manager);

        Assert.Equal(manager.Id, project.ProjectManagerId);
        Assert.Same(manager, project.ProjectManager);

        project.RemoveProjectManager();

        Assert.Null(project.ProjectManagerId);
        Assert.Null(project.ProjectManager);
    }

    private static Project CreateProject(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        return new Project(
            "Customer portal",
            "Customer LLC",
            "Contractor LLC",
            startDate ?? new DateTime(2026, 7, 1),
            endDate ?? new DateTime(2026, 7, 31),
            projectPriority: 1);
    }

    private static Employee CreateEmployee(int id)
    {
        return new Employee("Ivan", "Ivanov", "Ivanovich", $"ivan{id}@test.local")
        {
            Id = id
        };
    }
}
