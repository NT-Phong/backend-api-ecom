using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using Ecom.Domain.Constants;

namespace Ecom.Infrastructure.Logging;

public class HttpContextEnricher : ILogEventEnricher
{
    private static IHttpContextAccessor? _httpContextAccessor;
    
    public HttpContextEnricher()
    {
        // Parameterless constructor for Serilog
    }

    public HttpContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        // Remove static assignment to fix Sonar warning S3010
        // Use SetHttpContextAccessor method instead
    }
    
    public static void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.Request == null) return;

        // Add HTTP context properties with null safety
        var method = httpContext.Request?.Method ?? "";
        var path = httpContext.Request?.Path.Value ?? "";
        // Query strings may contain SignalR access_token or provider callback codes.
        var endpoint = $"{method} {path}";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Endpoint", endpoint));

        var statusCode = httpContext.Response?.StatusCode.ToString() ?? "";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("StatusCode", statusCode));
    }

}
