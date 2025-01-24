using System;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using Dapr.Framework.Domain.Services;

namespace Dapr.Framework.Application.Services;

/// <summary>
/// Implementation of distributed lock using Dapr
/// </summary>
public class DaprDistributedLockService : IDistributedLockService
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<DaprDistributedLockService> _logger;
    private readonly string _storeName;

    public DaprDistributedLockService(
        DaprClient daprClient,
        ILogger<DaprDistributedLockService> logger,
        string storeName = "lockstore")
    {
        _daprClient = daprClient;
        _logger = logger;
        _storeName = storeName;
    }

    public async Task<bool> TryAcquireLockAsync(string resourceId, int expiryInSeconds = 60)
    {
        try
        {
            var lockId = $"lock-{Guid.NewGuid()}";
            await using (var resourceLock = await _daprClient.Lock(_storeName, resourceId, lockId, expiryInSeconds))
            {
                if (resourceLock.Success)
                {
                    _logger.LogInformation("Successfully acquired lock for resource {ResourceId}", resourceId);
                    return true;
                }

                _logger.LogWarning("Failed to acquire lock for resource {ResourceId}", resourceId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring lock for resource {ResourceId}", resourceId);
            return false;
        }
    }

    public async Task<bool> ReleaseLockAsync(string resourceId)
    {
        try
        {
            var lockId = $"lock-{Guid.NewGuid()}";
            var unlockResponse = await _daprClient.Unlock(_storeName, resourceId, lockId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing lock for resource {ResourceId}", resourceId);
            return false;
        }
    }

    public async Task<T?> ExecuteWithLockAsync<T>(string resourceId, Func<Task<T>> function, int expiryInSeconds = 60)
    {
        try
        {
            var lockId = $"lock-{Guid.NewGuid()}";
            await using (var resourceLock = await _daprClient.Lock(_storeName, resourceId, lockId, expiryInSeconds))
            {
                if (!resourceLock.Success)
                {
                    _logger.LogWarning("Failed to acquire lock for resource {ResourceId}", resourceId);
                    return default;
                }

                _logger.LogInformation("Successfully acquired lock for resource {ResourceId}", resourceId);
                return await function();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function with lock for resource {ResourceId}", resourceId);
            return default;
        }
    }

    public async Task<bool> ExecuteWithLockAsync(string resourceId, Func<Task> action, int expiryInSeconds = 60)
    {
        try
        {
            var lockId = $"lock-{Guid.NewGuid()}";
            await using (var resourceLock = await _daprClient.Lock(_storeName, resourceId, lockId, expiryInSeconds))
            {
                if (!resourceLock.Success)
                {
                    _logger.LogWarning("Failed to acquire lock for resource {ResourceId}", resourceId);
                    return false;
                }

                _logger.LogInformation("Successfully acquired lock for resource {ResourceId}", resourceId);
                await action();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action with lock for resource {ResourceId}", resourceId);
            return false;
        }
    }
}
