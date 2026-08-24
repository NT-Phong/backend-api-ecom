using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Ecom.API.Middleware;

public class StructuredLoggingMiddleware
{
    private static readonly string[] HealthCheckPaths = ["/healthz", "/livez", "/readyz", "/metrics"];
    private const string CsrfCookieName = "__Host-ecom_csrf";
    private const string CartCookieName = "__Host-ecom_cart";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";
    private const string SecurityCsrfPath = "/api/v1/security/csrf";
    private const string CartPathPrefix = "/api/v1/cart";
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
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var isCsrfTokenRequest = HttpMethods.IsGet(context.Request.Method) &&
            string.Equals(requestPath, SecurityCsrfPath, StringComparison.OrdinalIgnoreCase);
        var isCartMutation = IsCartMutation(context);
        var hasCsrfRequestHeader = context.Request.Headers.ContainsKey(CsrfHeaderName);
        var hasCsrfRequestCookie = context.Request.Cookies.ContainsKey(CsrfCookieName);
        var hasCartRequestCookie = context.Request.Cookies.ContainsKey(CartCookieName);
        var hasAuthorizationHeader = context.Request.Headers.ContainsKey("Authorization");
        
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
                    LogCommerceCsrfDiagnostics(
                        context,
                        isCsrfTokenRequest,
                        isCartMutation,
                        hasCsrfRequestHeader,
                        hasCsrfRequestCookie,
                        hasCartRequestCookie,
                        hasAuthorizationHeader,
                        statusCode);

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

    private void LogCommerceCsrfDiagnostics(
        HttpContext context,
        bool isCsrfTokenRequest,
        bool isCartMutation,
        bool hasCsrfRequestHeader,
        bool hasCsrfRequestCookie,
        bool hasCartRequestCookie,
        bool hasAuthorizationHeader,
        int statusCode)
    {
        if (!isCsrfTokenRequest && !isCartMutation)
        {
            return;
        }

        var issuedCsrfCookie = context.Response.Headers.TryGetValue("Set-Cookie", out var setCookieHeaders) &&
            setCookieHeaders.Any(value => value.StartsWith($"{CsrfCookieName}=", StringComparison.OrdinalIgnoreCase));
        var principal = context.User;
        var isAuthenticated = principal?.Identity?.IsAuthenticated == true;
        var hasUserIdClaim = principal?.HasClaim(claim => claim.Type == "userId") == true;
        var hasNameIdentifierClaim = principal?.HasClaim(claim => claim.Type == System.Security.Claims.ClaimTypes.NameIdentifier) == true;
        var userIdClaim = principal?.FindFirst("userId")?.Value ??
            principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var hasValidUserIdClaim = Guid.TryParse(userIdClaim, out var userId) && userId != Guid.Empty;
        var authenticationType = context.User.Identity?.AuthenticationType ?? "None";

        _logger.Log(
            isCartMutation && statusCode >= StatusCodes.Status400BadRequest ? LogLevel.Warning : LogLevel.Information,
            "[CommerceCsrfDiagnostics] TraceId: {TraceId}; Flow: {Flow}; StatusCode: {StatusCode}; " +
            "HasCsrfRequestHeader: {HasCsrfRequestHeader}; CsrfRequestCookiePresent: {HasCsrfRequestCookie}; " +
            "CartRequestCookiePresent: {HasCartRequestCookie}; HasAuthorizationHeader: {HasAuthorizationHeader}; " +
            "IsAuthenticated: {IsAuthenticated}; AuthenticationType: {AuthenticationType}; " +
            "HasUserIdClaim: {HasUserIdClaim}; HasNameIdentifierClaim: {HasNameIdentifierClaim}; " +
            "HasValidUserIdClaim: {HasValidUserIdClaim}; CsrfCookieIssued: {IssuedCsrfCookie}",
            context.TraceIdentifier,
            isCsrfTokenRequest ? "IssueToken" : "CartMutation",
            statusCode,
            hasCsrfRequestHeader,
            hasCsrfRequestCookie,
            hasCartRequestCookie,
            hasAuthorizationHeader,
            isAuthenticated,
            authenticationType,
            hasUserIdClaim,
            hasNameIdentifierClaim,
            hasValidUserIdClaim,
            issuedCsrfCookie);
    }

    private static bool IsCartMutation(HttpContext context)
    {
        if (!(context.Request.Path.Value?.StartsWith(CartPathPrefix, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return false;
        }

        return HttpMethods.IsPost(context.Request.Method) ||
               HttpMethods.IsPatch(context.Request.Method) ||
               HttpMethods.IsDelete(context.Request.Method);
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
