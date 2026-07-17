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
        var clientIp = GetClientIp(httpContext);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", clientIp));

        var xForwardedIp = httpContext.Request.Headers[ApplicationConstants.HttpHeaders.ForwardedFor].FirstOrDefault() ?? "";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("XForwardedIp", xForwardedIp));

        var method = httpContext.Request?.Method ?? "";
        var path = httpContext.Request?.Path.Value ?? "";
        var queryString = httpContext.Request?.QueryString.Value ?? "";
        var endpoint = $"{method} {path}{queryString}";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Endpoint", endpoint));

        var statusCode = httpContext.Response?.StatusCode.ToString() ?? "";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("StatusCode", statusCode));
    }

    private static string GetClientIp(HttpContext context)
    {
        try
        {
            // Check X-Forwarded-For header first (for load balancers/proxies)
            var xForwardedFor = context.Request?.Headers[ApplicationConstants.HttpHeaders.ForwardedFor].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                // X-Forwarded-For can contain multiple IPs, take the first one
                return xForwardedFor.Split(',')[0].Trim();
            }

            // Check X-Real-IP header
            var xRealIp = context.Request?.Headers[ApplicationConstants.HttpHeaders.RealIp].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            // Fallback to connection remote IP
            return context.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }
}
