using System;
using System.Data;
using System.Threading.Tasks;
using Dapr.Framework.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Dapr.Framework.Infrastructure.Services;

/// <summary>
/// Provides transaction management for Entity Framework Core DbContext.
/// Handles database transactions with proper error handling and logging.
/// </summary>
public sealed class EfTransactionService : ITransactionService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<EfTransactionService> _logger;
    private IDbContextTransaction? _currentTransaction;
    private bool _disposed;

    public bool HasActiveTransaction => _currentTransaction != null;

    public EfTransactionService(
        DbContext dbContext,
        ILogger<EfTransactionService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Begins a new database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction. Defaults to ReadCommitted.</param>
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ThrowIfDisposed();
        ThrowIfTransactionActive();

        _logger.LogInformation("Beginning new transaction with isolation level: {IsolationLevel}", isolationLevel);
        try
        {
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel);
            _logger.LogDebug("Transaction successfully started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to begin transaction with isolation level: {IsolationLevel}", isolationLevel);
            throw;
        }
    }

    /// <summary>
    /// Commits the current transaction and saves all changes to the database.
    /// Automatically rolls back if any error occurs during the commit process.
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        ThrowIfDisposed();
        ThrowIfNoTransaction();

        _logger.LogInformation("Committing transaction");
        try
        {
            await _dbContext.SaveChangesAsync();
            await _currentTransaction!.CommitAsync();
            _logger.LogDebug("Transaction successfully committed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit transaction. Initiating rollback");
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Rolls back the current transaction, undoing all changes made within the transaction scope.
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        ThrowIfDisposed();
        ThrowIfNoTransaction();

        _logger.LogInformation("Rolling back transaction");
        try
        {
            await _currentTransaction!.RollbackAsync();
            _logger.LogDebug("Transaction successfully rolled back");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback transaction");
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing transaction service");
        if (_currentTransaction != null)
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing transaction service asynchronously");
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            _logger.LogError("Attempted to use disposed transaction service");
            throw new ObjectDisposedException(nameof(EfTransactionService));
        }
    }

    private void ThrowIfTransactionActive()
    {
        if (HasActiveTransaction)
        {
            _logger.LogError("Attempted to begin a transaction while another is in progress");
            throw new InvalidOperationException("A transaction is already in progress");
        }
    }

    private void ThrowIfNoTransaction()
    {
        if (!HasActiveTransaction)
        {
            _logger.LogError("Attempted to perform operation with no active transaction");
            throw new InvalidOperationException("No active transaction");
        }
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            _logger.LogDebug("Disposing current transaction");
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
