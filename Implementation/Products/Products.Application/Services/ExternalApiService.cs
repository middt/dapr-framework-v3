using Dapr.Client;
using Microsoft.Extensions.Logging;
using Dapr.Framework.Application.Services;
using Polly;
using System.Net.Http;
using Workflow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Products.Application.Services;

/// <summary>
/// Service for making external API calls using Dapr service invocation with resiliency patterns
/// </summary>
public class ExternalApiService : DaprExternalService
{
    private readonly ILogger<ExternalApiService> _logger;
    private readonly WorkflowDbContext _dbContext;

    public ExternalApiService(
        DaprClient daprClient,
        ILogger<ExternalApiService> logger,
        WorkflowDbContext dbContext)
        : base(daprClient, logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<object?> InvokeWithResiliencyAsync<TRequest, TResponse>(
        string serviceName,
        string operation,
        TRequest request)
        where TRequest : class
        where TResponse : class
    {
        var apiConfig = await _dbContext.ExternalApiConfigs
            .FirstOrDefaultAsync(c => c.Method == "GET" && c.Url.Contains(serviceName));

        if (apiConfig == null)
        {
            _logger.LogWarning("No API configuration found for {ServiceName}. Using default settings.", serviceName);
            return await InvokeAsync<TRequest, TResponse>(serviceName, operation, request);
        }

        var retrySettings = apiConfig.RetryPolicy ?? new { MaxRetries = 3, RetryTimeout = 1000 };
        var maxRetries = retrySettings.MaxRetries;
        var retryTimeout = retrySettings.RetryTimeout;

        var policy = Policy<object?>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                maxRetries,
                retryAttempt => TimeSpan.FromMilliseconds(retryTimeout * Math.Pow(2, retryAttempt - 1)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Retry {RetryCount} of {MaxRetries} after {Delay}ms",
                        retryCount,
                        maxRetries,
                        timeSpan.TotalMilliseconds);
                });

        return await policy.ExecuteAsync(async () =>
        {
            try
            {
                _logger.LogInformation(
                    "Making external API call to {Service}/{Operation}",
                    serviceName,
                    operation);

                var response = await InvokeAsync<TRequest, TResponse>(
                    serviceName,
                    operation,
                    request);

                _logger.LogInformation("External API call successful");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making external API call");
                throw;
            }
        });
    }
}
