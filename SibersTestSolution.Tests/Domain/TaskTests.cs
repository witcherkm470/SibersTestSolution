using DomainTask = SibersTestSolution.Domain.Entities.Task;
using TaskStatus = SibersTestSolution.Domain.Enums.TaskStatus;

namespace SibersTestSolution.Tests.Domain;

public sealed class TaskTests
{
    [Fact]
    public void Constructor_CreatesTaskWithTodoStatus()
    {
        var task = CreateTask();

        Assert.Equal(TaskStatus.Todo, task.TaskStatus);
    }

    [Fact]
    public void ChangeStatus_DoesNotAllowMovingFromTodoDirectlyToDone()
    {
        var task = CreateTask();

        var exception = Assert.Throws<InvalidOperationException>(() => task.ChangeStatus(TaskStatus.Done));

        Assert.Contains("Todo", exception.Message);
        Assert.Contains("Done", exception.Message);
        Assert.Equal(TaskStatus.Todo, task.TaskStatus);
    }

    [Fact]
    public void ChangeStatus_AllowsMovingFromInProgressToTodoAndDone()
    {
        var task = CreateTask();

        task.ChangeStatus(TaskStatus.InProgress);
        task.ChangeStatus(TaskStatus.Todo);
        task.ChangeStatus(TaskStatus.InProgress);
        task.ChangeStatus(TaskStatus.Done);

        Assert.Equal(TaskStatus.Done, task.TaskStatus);
    }

    [Fact]
    public void ChangeStatus_DoesNotAllowChangingFinishedTask()
    {
        var task = CreateTask();
        task.ChangeStatus(TaskStatus.InProgress);
        task.ChangeStatus(TaskStatus.Done);

        Assert.Throws<InvalidOperationException>(() => task.ChangeStatus(TaskStatus.InProgress));
        Assert.Equal(TaskStatus.Done, task.TaskStatus);
    }

    [Theory]
    [InlineData(TaskStatus.Default)]
    [InlineData((TaskStatus)99)]
    public void ChangeStatus_RejectsInvalidStatus(TaskStatus status)
    {
        var task = CreateTask();

        Assert.Throws<ArgumentException>(() => task.ChangeStatus(status));
    }

    [Fact]
    public void Constructor_RejectsNonPositivePriority()
    {
        Assert.Throws<ArgumentException>(() => CreateTask(taskPriority: 0));
    }

    private static DomainTask CreateTask(int taskPriority = 1)
    {
        return new DomainTask(
            "Prepare report",
            projectId: 1,
            taskOwnerId: 1,
            taskPerformerId: null,
            comment: null,
            taskPriority);
    }
}
