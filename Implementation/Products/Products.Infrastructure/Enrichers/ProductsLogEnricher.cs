using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Dapr.Framework.Telemetry.Enrichers;

namespace Products.Infrastructure.Enrichers;

public class ProductsLogEnricher : HeaderLogEnricher
{
    private readonly string _customName;

    public ProductsLogEnricher(
        string customName,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(configuration, httpContextAccessor)
    {
        _customName =customName;
    }

    public override void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // First, enrich with header values from base class
        base.Enrich(logEvent, propertyFactory);

        // Add custom name property
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CustomName", _customName));
    }
}
