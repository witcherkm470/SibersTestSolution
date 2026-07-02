using Microsoft.EntityFrameworkCore.Storage;
using SibersTestSolution.Infrastructure.Abstractions;

namespace SibersTestSolution.Infrastructure.Internal;

/// <summary>
/// Entity Framework transaction-backed unit of work.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWorkDbContext _dbContext;
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    /// <summary>
    /// Creates a unit of work for an existing transaction.
    /// </summary>
    public UnitOfWork(IUnitOfWorkDbContext dbContext, IDbContextTransaction transaction)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public int SaveChanges()
    {
        ThrowIfDisposed();
        ThrowIfCompleted();

        return _dbContext.SaveChanges();
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ThrowIfCompleted();

        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Commit()
    {
        ThrowIfDisposed();
        ThrowIfCompleted();

        _dbContext.SaveChanges();
        _transaction.Commit();

        IsCompleted = true;
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ThrowIfCompleted();

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);

        IsCompleted = true;
    }

    /// <inheritdoc />
    public void Rollback()
    {
        ThrowIfDisposed();

        if (IsCompleted)
        {
            return;
        }

        _transaction.Rollback();

        IsCompleted = true;
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (IsCompleted)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);

        IsCompleted = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (!IsCompleted)
        {
            Rollback();
        }

        _transaction.Dispose();

        _disposed = true;
    }

    private void ThrowIfCompleted()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException("Unit of work is already completed.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
