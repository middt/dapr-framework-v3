using Serilog.Core;
using Serilog.Events;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Dapr.Framework.Telemetry.Enrichers;

public class HeaderLogEnricher : ILogEventEnricher
{
    private readonly IEnumerable<string> _headerNames;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HeaderLogEnricher(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _headerNames = configuration.GetSection("Telemetry:Logging:Enrichers:Headers").Get<string[]>() ?? Array.Empty<string>();
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        foreach (var headerName in _headerNames)
        {
            var headerValue = httpContext.Request.Headers[headerName].ToString();
            if (!string.IsNullOrEmpty(headerValue))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(headerName, headerValue));
            }
        }
    }
}
