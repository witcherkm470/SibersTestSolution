using SibersTestSolution.Application.Abstractions;
using SibersTestSolution.Application.Common;
using SibersTestSolution.Application.DTOs.Tasks;
using SibersTestSolution.Tests.Support;
using Employee = SibersTestSolution.Domain.Entities.Employee;
using DomainTask = SibersTestSolution.Domain.Entities.Task;
using Project = SibersTestSolution.Domain.Entities.Project;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Tests.Application;

public sealed class TaskServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesTodoTaskAndCommits()
    {
        var owner = CreateEmployee(1, "owner@test.local");
        var performer = CreateEmployee(2, "performer@test.local");
        var project = CreateProject(1);
        var taskRepository = new FakeTaskRepository();
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            taskRepository,
            unitOfWorkManager,
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(owner, performer));

        var response = await service.CreateAsync(new CreateTaskRequest(
            " Prepare report ",
            project.Id,
            owner.Id,
            performer.Id,
            " Comment ",
            3));

        Assert.True(taskRepository.AddCalled);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
        Assert.Equal("Prepare report", response.Name);
        Assert.Equal(TaskStatus.Todo, response.TaskStatus);
        Assert.Equal(performer.Id, response.TaskPerformerId);
        Assert.Equal("Comment", response.Comment);
    }

    [Fact]
    public async Task CreateAsync_RejectsDeletedOwnerAndRollsBack()
    {
        var owner = CreateEmployee(1, "deleted-owner@test.local");
        owner.MarkDeleted();
        var project = CreateProject(1);

        var taskRepository = new FakeTaskRepository();
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            taskRepository,
            unitOfWorkManager,
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(owner));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(new CreateTaskRequest("Task", project.Id, owner.Id, null, null, 1)));

        Assert.False(taskRepository.AddCalled);
        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task CreateAsync_RejectsDeletedPerformerAndRollsBack()
    {
        var owner = CreateEmployee(1, "owner@test.local");
        var performer = CreateEmployee(2, "deleted-performer@test.local");
        performer.MarkDeleted();
        var project = CreateProject(1);

        var taskRepository = new FakeTaskRepository();
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            taskRepository,
            unitOfWorkManager,
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(owner, performer));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(new CreateTaskRequest("Task", project.Id, owner.Id, performer.Id, null, 1)));

        Assert.False(taskRepository.AddCalled);
        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task UpdateAsync_RemovesPerformerWhenRequestHasNoPerformer()
    {
        var owner = CreateEmployee(1, "owner@test.local");
        var performer = CreateEmployee(2, "performer@test.local");
        var project = CreateProject(1);
        var task = CreateTask();
        task.AssignPerformer(performer.Id);

        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(
            new FakeTaskRepository(task),
            unitOfWorkManager,
            new FakeProjectRepository(project),
            new FakeEmployeeRepository(owner, performer));

        var response = await service.UpdateAsync(
            task.Id,
            new UpdateTaskRequest("Updated", project.Id, owner.Id, null, TaskStatus.Todo, null, 10));

        Assert.Null(response.TaskPerformerId);
        Assert.Null(task.TaskPerformerId);
        Assert.Equal("Updated", task.Name);
        Assert.Equal(10, task.TaskPriority);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task ChangeStatusAsync_AllowsInProgressToDoneAndCommits()
    {
        var task = CreateTask();
        task.ChangeStatus(TaskStatus.InProgress);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(new FakeTaskRepository(task), unitOfWorkManager);

        var response = await service.ChangeStatusAsync(task.Id, new ChangeTaskStatusRequest(TaskStatus.Done));

        Assert.Equal(TaskStatus.Done, response.TaskStatus);
        Assert.Equal(TaskStatus.Done, task.TaskStatus);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task ChangeStatusAsync_RejectsDoneToTodoAndRollsBack()
    {
        var task = CreateTask();
        task.ChangeStatus(TaskStatus.InProgress);
        task.ChangeStatus(TaskStatus.Done);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(new FakeTaskRepository(task), unitOfWorkManager);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeStatusAsync(task.Id, new ChangeTaskStatusRequest(TaskStatus.Todo)));

        Assert.Equal(TaskStatus.Done, task.TaskStatus);
        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTodoTaskAndCommits()
    {
        var task = CreateTask();
        var taskRepository = new FakeTaskRepository(task);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(taskRepository, unitOfWorkManager);

        await service.DeleteAsync(task.Id);

        Assert.True(taskRepository.RemoveCalled);
        Assert.True(unitOfWorkManager.Current.CommitCalled);
        Assert.False(unitOfWorkManager.Current.RollbackCalled);
    }

    [Fact]
    public async Task DeleteAsync_RejectsTaskThatIsNotTodo()
    {
        var task = CreateTask();
        task.ChangeStatus(TaskStatus.InProgress);

        var taskRepository = new FakeTaskRepository(task);
        var unitOfWorkManager = new FakeUnitOfWorkManager();
        var service = CreateService(taskRepository, unitOfWorkManager);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(task.Id));

        Assert.False(taskRepository.RemoveCalled);
        Assert.False(unitOfWorkManager.Current.CommitCalled);
        Assert.True(unitOfWorkManager.Current.RollbackCalled);
    }

    private static ITaskService CreateService(
        FakeTaskRepository taskRepository,
        FakeUnitOfWorkManager unitOfWorkManager,
        FakeProjectRepository? projectRepository = null,
        FakeEmployeeRepository? employeeRepository = null)
    {
        var serviceType = typeof(ITaskService).Assembly.GetType("SibersTestSolution.Application.Services.TaskService")
            ?? throw new InvalidOperationException("TaskService type was not found.");

        var service = Activator.CreateInstance(
            serviceType,
            taskRepository,
            projectRepository ?? new FakeProjectRepository(),
            employeeRepository ?? new FakeEmployeeRepository(),
            unitOfWorkManager);

        return (ITaskService)(service ?? throw new InvalidOperationException("TaskService was not created."));
    }

    private static DomainTask CreateTask()
    {
        return new DomainTask(
            "Prepare report",
            projectId: 1,
            taskOwnerId: 1,
            taskPerformerId: null,
            comment: null,
            taskPriority: 1)
        {
            Id = 1
        };
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
