using SibersTestSolution.Infrastructure.Abstractions;

namespace SibersTestSolution.Tests.Support;

internal sealed class FakeUnitOfWorkManager : IUnitOfWorkManager
{
    public FakeUnitOfWork Current { get; private set; } = new();

    public IUnitOfWork Create()
    {
        Current = new FakeUnitOfWork();
        return Current;
    }
}
