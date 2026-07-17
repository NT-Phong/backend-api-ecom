using Microsoft.AspNetCore.Http;

namespace Ecom.API.Middleware;

public class ProxyAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProxyAuthorizationMiddleware> _logger;

    public ProxyAuthorizationMiddleware(RequestDelegate next, ILogger<ProxyAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for health checks, metrics, and Swagger endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path != null && (path.StartsWith("/healthz") || path.StartsWith("/livez") || 
                           path.StartsWith("/readyz") || path.StartsWith("/metrics") ||
                           path.StartsWith("/swagger") || path == "/"))
        {
            await _next(context);
            return;
        }
        await _next(context);
    }

    private static string? GetHeaderValue(HttpContext context, string headerName)
    {
        // Check for exact match first
        if (context.Request.Headers.TryGetValue(headerName, out var exactMatch))
        {
            return exactMatch.FirstOrDefault();
        }

        // Check for case-insensitive match
        var header = context.Request.Headers
            .FirstOrDefault(h => string.Equals(h.Key, headerName, StringComparison.OrdinalIgnoreCase));
        
        return header.Value.FirstOrDefault();
    }
}

