using Dapr.Client;
using Dapr.Framework.Domain.Caching;
using Dapr.Framework.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Framework.Api.Configuration
{
    public static class CachingConfiguration
    {
        /// <summary>
        /// Add .NET Core Distributed Cache service
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configureCache">Optional action to configure the distributed cache implementation</param>
        public static IServiceCollection AddNetCoreDistributedCache(
            this IServiceCollection services, 
            Action<IServiceCollection>? configureCache = null)
        {
            // Allow external configuration of distributed cache
            configureCache?.Invoke(services);

            // Register the cache service
            services.AddScoped<IDistributedCacheService, NetCoreDistributedCacheService>();

            return services;
        }

        /// <summary>
        /// Add Dapr State Store Cache service
        /// </summary>
        public static IServiceCollection AddDaprStateStoreCache(
            this IServiceCollection services, 
            string storeName = "statestore")
        {
            // Ensure Dapr client is registered
            services.AddDaprClient();

            // Register the Dapr State Store cache service
            services.AddScoped<IDistributedCacheService>(sp => 
                new DaprStateStoreCacheService(
                    sp.GetRequiredService<DaprClient>(), 
                    storeName
                )
            );

            return services;
        }
    }
}
