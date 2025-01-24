using System;
using System.Threading.Tasks;
using Dapr.Framework.Domain.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Dapr.Framework.Application.Services.Redis;

/// <summary>
/// Implementation of distributed lock using Redis
/// </summary>
public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<RedisDistributedLockService> _logger;
    private readonly TimeSpan _defaultLockTimeout = TimeSpan.FromSeconds(60);

    public RedisDistributedLockService(
        IConnectionMultiplexer redisConnection,
        ILogger<RedisDistributedLockService> logger)
    {
        _redisConnection = redisConnection ?? 
            throw new ArgumentNullException(nameof(redisConnection));
        _logger = logger ?? 
            throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> TryAcquireLockAsync(string resourceId, int expiryInSeconds = 60)
    {
        try
        {
            var database = _redisConnection.GetDatabase();
            var lockKey = $"distributed-lock:{resourceId}";
            var lockValue = Guid.NewGuid().ToString(); // Unique identifier for this lock attempt
            var expiry = TimeSpan.FromSeconds(expiryInSeconds);

            // Use atomic SETNX (SET if Not eXists) with expiry
            bool acquired = await database.StringSetAsync(
                lockKey, 
                lockValue, 
                expiry, 
                When.NotExists
            );

            if (acquired)
            {
                _logger.LogInformation("Successfully acquired Redis lock for resource {ResourceId}", resourceId);
                return true;
            }

            _logger.LogWarning("Failed to acquire Redis lock for resource {ResourceId}", resourceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring Redis lock for resource {ResourceId}", resourceId);
            return false;
        }
    }

    public async Task<bool> ReleaseLockAsync(string resourceId)
    {
        try
        {
            var database = _redisConnection.GetDatabase();
            var lockKey = $"distributed-lock:{resourceId}";

            // Use Lua script to ensure atomic delete only if the lock exists
            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            var lockValue = await database.StringGetAsync(lockKey);
            if (lockValue.IsNullOrEmpty)
            {
                _logger.LogWarning("Attempting to release non-existent lock for resource {ResourceId}", resourceId);
                return false;
            }

            var result = await database.ScriptEvaluateAsync(script, 
                new RedisKey[] { lockKey }, 
                new RedisValue[] { lockValue }
            );

            bool released = (int)result == 1;
            
            if (released)
            {
                _logger.LogInformation("Successfully released Redis lock for resource {ResourceId}", resourceId);
            }
            else
            {
                _logger.LogWarning("Failed to release Redis lock for resource {ResourceId}", resourceId);
            }

            return released;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing Redis lock for resource {ResourceId}", resourceId);
            return false;
        }
    }

    public async Task<T?> ExecuteWithLockAsync<T>(string resourceId, Func<Task<T>> function, int expiryInSeconds = 60)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        bool lockAcquired = await TryAcquireLockAsync(resourceId, expiryInSeconds);
        
        if (!lockAcquired)
        {
            _logger.LogWarning("Could not acquire lock for resource {ResourceId}", resourceId);
            return default;
        }

        try
        {
            return await function();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing function with Redis lock for resource {ResourceId}", resourceId);
            throw;
        }
        finally
        {
            await ReleaseLockAsync(resourceId);
        }
    }

    public async Task<bool> ExecuteWithLockAsync(string resourceId, Func<Task> action, int expiryInSeconds = 60)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        bool lockAcquired = await TryAcquireLockAsync(resourceId, expiryInSeconds);
        
        if (!lockAcquired)
        {
            _logger.LogWarning("Could not acquire lock for resource {ResourceId}", resourceId);
            return false;
        }

        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action with Redis lock for resource {ResourceId}", resourceId);
            throw;
        }
        finally
        {
            await ReleaseLockAsync(resourceId);
        }
    }
}
