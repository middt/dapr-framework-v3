using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Framework.Domain.Caching;
using Microsoft.Extensions.Caching.Distributed;
using DomainDistributedCacheEntryOptions = Dapr.Framework.Domain.Caching.DistributedCacheEntryOptions;
using MicrosoftDistributedCacheEntryOptions = Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions;

namespace Dapr.Framework.Infrastructure.Caching
{
    /// <summary>
    /// Implementation of distributed cache using .NET Core's IDistributedCache
    /// </summary>
    public class NetCoreDistributedCacheService : DistributedCacheBase
    {
        private readonly IDistributedCache _distributedCache;
        private readonly JsonSerializerOptions _jsonOptions;

        public NetCoreDistributedCacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public override async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            var cachedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
            
            if (string.IsNullOrEmpty(cachedValue))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public override async Task SetAsync<T>(
            string key, 
            T value, 
            DomainDistributedCacheEntryOptions? options = null, 
            CancellationToken cancellationToken = default) where T : class
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            
            var cacheOptions = new MicrosoftDistributedCacheEntryOptions();
            
            if (options?.AbsoluteExpiration.HasValue == true)
            {
                cacheOptions.SetAbsoluteExpiration(options.AbsoluteExpiration.Value);
            }
            
            if (options?.SlidingExpiration.HasValue == true)
            {
                cacheOptions.SetSlidingExpiration(options.SlidingExpiration.Value);
            }

            await _distributedCache.SetStringAsync(key, serializedValue, cacheOptions, cancellationToken);
        }

        public override Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _distributedCache.RemoveAsync(key, cancellationToken);
        }

        public override Task RefreshAsync(string key, CancellationToken cancellationToken = default)
        {
            return _distributedCache.RefreshAsync(key, cancellationToken);
        }
    }
}
