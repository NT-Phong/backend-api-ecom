using Microsoft.AspNetCore.Http;

namespace Ecom.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing request
        AddSecurityHeaders(context);
        
        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var response = context.Response;
        
        // X-Content-Type-Options: Prevent MIME type sniffing
        if (!response.Headers.ContainsKey("X-Content-Type-Options"))
        {
            response.Headers.Append("X-Content-Type-Options", "nosniff");
        }
        
        // X-Frame-Options: Prevent clickjacking
        if (!response.Headers.ContainsKey("X-Frame-Options"))
        {
            response.Headers.Append("X-Frame-Options", "DENY");
        }
        
        // Referrer-Policy: Control referrer information
        if (!response.Headers.ContainsKey("Referrer-Policy"))
        {
            response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        }
        
        // X-XSS-Protection: Enable XSS filtering
        if (!response.Headers.ContainsKey("X-XSS-Protection"))
        {
            response.Headers.Append("X-XSS-Protection", "1; mode=block");
        }
        
        // Content-Security-Policy: Basic CSP for API
        if (!response.Headers.ContainsKey("Content-Security-Policy"))
        {
            response.Headers.Append("Content-Security-Policy", "default-src 'self'; frame-ancestors 'none';");
        }
        
        // Permissions-Policy: Restrict browser features
        if (!response.Headers.ContainsKey("Permissions-Policy"))
        {
            response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
        }
        
        // Cache-Control: Prevent caching of sensitive responses
        if (!response.Headers.ContainsKey("Cache-Control"))
        {
            response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        }
        
        // Pragma: Legacy cache control
        if (!response.Headers.ContainsKey("Pragma"))
        {
            response.Headers.Append("Pragma", "no-cache");
        }
        
        // Note: HSTS header intentionally omitted for HTTP-only deployment
        // If HTTPS is enabled later, add:
        // response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        
        _logger.LogDebug("Security headers added to response for {RequestPath}", context.Request.Path);
    }
}
