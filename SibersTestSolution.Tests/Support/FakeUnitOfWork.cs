using SibersTestSolution.Infrastructure.Abstractions;

namespace SibersTestSolution.Tests.Support;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public bool IsCompleted { get; private set; }

    public bool CommitCalled { get; private set; }

    public bool RollbackCalled { get; private set; }

    public int SaveChanges()
    {
        return 0;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public void Commit()
    {
        CommitCalled = true;
        IsCompleted = true;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        Commit();
        return Task.CompletedTask;
    }

    public void Rollback()
    {
        RollbackCalled = true;
        IsCompleted = true;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        Rollback();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!IsCompleted)
        {
            Rollback();
        }
    }
}
