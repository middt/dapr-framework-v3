using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapr.Framework.Domain.Services;

public interface ITransactionService : IDisposable, IAsyncDisposable
{
    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    bool HasActiveTransaction { get; }
}
