using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Dapr.Framework.Api.Configuration;

public class RedisConfiguration
{
    public string Mode { get; set; } = "Standalone";
    public string InstanceName { get; set; } = "default";
    public int ConnectionTimeout { get; set; } = 5000;
    public int DefaultDatabase { get; set; } = 0;
    public string Password { get; set; } = "";
    public bool Ssl { get; set; } = false;
    public StandaloneConfig Standalone { get; set; } = new();
    public ClusterConfig Cluster { get; set; } = new();
    public SentinelConfig Sentinel { get; set; } = new();
    public RetryPolicyConfig RetryPolicy { get; set; } = new();
}

public class StandaloneConfig
{
    public List<string> EndPoints { get; set; } = new();
}

public class ClusterConfig
{
    public List<string> EndPoints { get; set; } = new();
    public int MaxRedirects { get; set; } = 3;
}

public class SentinelConfig
{
    public List<string> Masters { get; set; } = new();
    public List<string> Sentinels { get; set; } = new();
    public int DefaultDatabase { get; set; } = 0;
}

public class RetryPolicyConfig
{
    public int MaxRetries { get; set; } = 3;
    public int RetryTimeout { get; set; } = 1000;
}

public static class RedisConfigurationExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Redis").Get<RedisConfiguration>();
        
        var configurationOptions = new ConfigurationOptions
        {
            DefaultDatabase = redisConfig.DefaultDatabase,
            Password = redisConfig.Password,
            Ssl = redisConfig.Ssl,
            ConnectTimeout = redisConfig.ConnectionTimeout,
            AbortOnConnectFail = false
        };

        switch (redisConfig.Mode.ToLower())
        {
            case "standalone":
                foreach (var endpoint in redisConfig.Standalone.EndPoints)
                {
                    configurationOptions.EndPoints.Add(endpoint);
                }
                break;

            case "cluster":
                configurationOptions.CommandMap = CommandMap.Create(new HashSet<string> { "PING" }, false);
                foreach (var endpoint in redisConfig.Cluster.EndPoints)
                {
                    configurationOptions.EndPoints.Add(endpoint);
                }
                // configurationOptions.MaxRedirects = redisConfig.Cluster.MaxRedirects;
                break;

            case "sentinel":
                configurationOptions.ServiceName = redisConfig.Sentinel.Masters[0];
                foreach (var sentinel in redisConfig.Sentinel.Sentinels)
                {
                    configurationOptions.EndPoints.Add(sentinel);
                }
                configurationOptions.TieBreaker = "";
                break;

            default:
                throw new ArgumentException($"Unsupported Redis mode: {redisConfig.Mode}");
        }

        var multiplexer = ConnectionMultiplexer.Connect(configurationOptions);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        /*
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configurationOptions.ToString();
            options.InstanceName = redisConfig.InstanceName;
        });
        */
        return services;
    }
}