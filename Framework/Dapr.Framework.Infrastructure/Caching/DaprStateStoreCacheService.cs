using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Dapr.Framework.Domain.Caching;

namespace Dapr.Framework.Infrastructure.Caching
{
    /// <summary>
    /// Implementation of distributed cache using Dapr State Store
    /// </summary>
    public class DaprStateStoreCacheService : DistributedCacheBase
    {
        private readonly DaprClient _daprClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _storeName;

        public DaprStateStoreCacheService(
            DaprClient daprClient, 
            string storeName = "statestore")
        {
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
            _storeName = storeName;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public override async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                return await _daprClient.GetStateAsync<T?>(_storeName, key, cancellationToken: cancellationToken);
            }
            catch
            {
                return default;
            }
        }

        public override async Task SetAsync<T>(
            string key, 
            T value, 
            DistributedCacheEntryOptions? options = null, 
            CancellationToken cancellationToken = default) where T : class
        {
            var metadata = new Dictionary<string, string>();

            // Add TTL if specified
            if (options?.AbsoluteExpiration.HasValue == true)
            {
                var ttl = (int)(options.AbsoluteExpiration.Value - DateTimeOffset.UtcNow).TotalSeconds;
                if (ttl > 0)
                {
                    metadata["ttlInSeconds"] = ttl.ToString();
                }
            }
            else if (options?.SlidingExpiration.HasValue == true)
            {
                metadata["ttlInSeconds"] = ((int)options.SlidingExpiration.Value.TotalSeconds).ToString();
            }

            await _daprClient.SaveStateAsync(
                _storeName, 
                key, 
                value, 
                metadata: metadata, 
                cancellationToken: cancellationToken
            );
        }

        public override Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _daprClient.DeleteStateAsync(_storeName, key, cancellationToken: cancellationToken);
        }

        public override Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            // Dapr doesn't have a direct refresh mechanism, so we'll do nothing
            return Task.CompletedTask;
        }
    }
}
