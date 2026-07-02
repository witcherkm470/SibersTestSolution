using SibersTestSolution.Infrastructure.Abstractions;

namespace SibersTestSolution.Infrastructure.Internal;

/// <summary>
/// Internal factory for creating transactional units of work.
/// </summary>
internal interface IUnitOfWorkFactory
{
    /// <summary>
    /// Creates a new unit of work.
    /// </summary>
    IUnitOfWork Create();
}
