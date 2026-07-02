using SibersTestSolution.Infrastructure.Abstractions;

namespace SibersTestSolution.Infrastructure.Internal;

/// <summary>
/// Default unit-of-work manager implementation.
/// </summary>
internal sealed class UnitOfWorkManager : IUnitOfWorkManager
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    /// <summary>
    /// Creates a unit-of-work manager.
    /// </summary>
    public UnitOfWorkManager(IUnitOfWorkFactory unitOfWorkFactory)
    {
        _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
    }

    /// <inheritdoc />
    public IUnitOfWork Create()
    {
        return _unitOfWorkFactory.Create();
    }
}
