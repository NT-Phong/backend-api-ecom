using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Ecom.API.Middleware;

public class StructuredLoggingMiddleware
{
    private static readonly string[] HealthCheckPaths = ["/healthz", "/livez", "/readyz", "/metrics"];
    private readonly RequestDelegate _next;
    private readonly ILogger<StructuredLoggingMiddleware> _logger;

    public StructuredLoggingMiddleware(RequestDelegate next, ILogger<StructuredLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var isHealthCheck = IsHealthCheckRequest(context);
        
        Exception? exception = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;
            
            // Add latency and error info to LogContext for enrichers to pick up
            using (LogContext.PushProperty("Latency", $"{elapsed}ms"))
            using (LogContext.PushProperty("ExceptionType", exception?.GetType().Name ?? ""))
            {
                if (exception != null)
                {
                    _logger.LogError("HTTP request failed. ExceptionType: {ExceptionType}", exception.GetType().Name);
                }
                else
                {
                    var statusCode = context.Response.StatusCode;
                    if (statusCode >= 400)
                    {
                        _logger.LogWarning("HTTP request completed with warning status");
                    }
                    else if (!isHealthCheck)
                    {
                        _logger.LogInformation("HTTP request completed successfully");
                    }
                }
            }
        }
    }

    private static bool IsHealthCheckRequest(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            return false;
        }

        var pathValue = context.Request.Path.Value;
        if (string.IsNullOrEmpty(pathValue))
        {
            return false;
        }

        return HealthCheckPaths.Any(checkPath => pathValue.StartsWith(checkPath, StringComparison.OrdinalIgnoreCase));
    }
}
