using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Exporter;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Dapr.Framework.Telemetry.Configuration;

public static class TelemetryConfiguration
{
    public static IServiceCollection AddFrameworkTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<LoggerConfiguration, TelemetryOptions>? configureLogger = null)
    {
        var options = configuration.GetSection("Telemetry").Get<TelemetryOptions>() ?? new TelemetryOptions();

        ConfigureTraceProvider(services, options);
        ConfigureLogProvider(services, options, configureLogger);

        return services;
    }

    private static void ConfigureTraceProvider(IServiceCollection services, TelemetryOptions options)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .AddSource(options.ServiceName)
                    .AddSource("Dapr.Client")
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: options.ServiceName, serviceVersion: options.ServiceVersion)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                ["environment"] = options.Environment,
                                ["service.name"] = options.ServiceName,
                                ["service.version"] = options.ServiceVersion,
                                ["deployment.id"] = GetDeploymentId(options)
                            }))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(efOptions =>
                    {
                        efOptions.SetDbStatementForText = true;
                        efOptions.SetDbStatementForStoredProcedure = true;
                    });

                if (options.TraceProvider?.ToLower() == "zipkin")
                {
                    builder.AddZipkinExporter(zipkinOptions =>
                    {
                        zipkinOptions.Endpoint = new Uri(options.Zipkin.Endpoint);
                    });
                }
                else if (options.TraceProvider?.ToLower() == "otlp")
                {
                    builder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.Otlp.Endpoint);
                    });
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName: options.ServiceName, serviceVersion: options.ServiceVersion))
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation();

                switch (options.TraceProvider.ToLower())
                {
                    case "otlp":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.Otlp.Endpoint);
                        });
                        break;

                    case "elastic":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.Elastic.Endpoint);
                            if (!string.IsNullOrEmpty(options.Elastic.Username))
                            {
                                otlpOptions.Headers = $"Authorization=Bearer {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.Elastic.Username}:{options.Elastic.Password}"))}";
                            }
                        });
                        break;

                    case "openobserve":
                        builder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(options.OpenObserve.Endpoint);
                            if (!string.IsNullOrEmpty(options.OpenObserve.Username))
                            {
                                otlpOptions.Headers = $"Authorization=Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.OpenObserve.Username}:{options.OpenObserve.Password}"))}";
                            }
                        });
                        break;
                }
            });
    }

    private static void ConfigureLogProvider(IServiceCollection services, TelemetryOptions options, Action<LoggerConfiguration, TelemetryOptions>? configureLogger = null)
    {
        if (!options.Logging.Enabled)
            return;

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(ConvertLogLevel(options.Logging.MinimumLevel))
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", options.Environment)
            .Enrich.WithProperty("ServiceName", options.ServiceName)
            .Enrich.WithProperty("ServiceVersion", options.ServiceVersion)
            .Enrich.WithProperty("DeploymentId", GetDeploymentId(options));

        // Configure file logging
        loggerConfig.WriteTo.File(
            path: options.Logging.FilePath,
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // Configure console logging
        loggerConfig.WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        configureLogger?.Invoke(loggerConfig, options);

        Log.Logger = loggerConfig.CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
    }

    private static LogEventLevel ConvertLogLevel(LogLevel level) => level switch
    {
        LogLevel.Trace => LogEventLevel.Verbose,
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Critical => LogEventLevel.Fatal,
        _ => LogEventLevel.Information
    };

    public static string GetDeploymentId(TelemetryOptions options) => $"{options.Environment}-{options.ServiceName}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
}
