using SibersTestSolution.Infrastructure.Abstractions;

namespace SibersTestSolution.Infrastructure.Internal;

/// <summary>
/// Creates unit-of-work instances from the current database context.
/// </summary>
internal sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IUnitOfWorkDbContext _dbContext;

    /// <summary>
    /// Creates a unit-of-work factory.
    /// </summary>
    public UnitOfWorkFactory(IUnitOfWorkDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public IUnitOfWork Create()
    {
        var transaction = _dbContext.BeginTransaction();

        return new UnitOfWork(_dbContext, transaction);
    }
}
