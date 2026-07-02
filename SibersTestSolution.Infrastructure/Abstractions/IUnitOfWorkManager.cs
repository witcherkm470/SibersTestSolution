namespace SibersTestSolution.Infrastructure.Abstractions;

/// <summary>
/// Creates units of work for application operations.
/// </summary>
public interface IUnitOfWorkManager
{
    /// <summary>
    /// Creates a new unit of work.
    /// </summary>
    IUnitOfWork Create();
}
