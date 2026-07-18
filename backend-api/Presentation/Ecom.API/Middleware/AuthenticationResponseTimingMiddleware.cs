using Ecom.Application.Common.Configuration;
using Microsoft.Extensions.Options;

namespace Ecom.API.Middleware;

public sealed class AuthenticationResponseTimingMiddleware(
    RequestDelegate next,
    IOptions<AuthResponseTimingOptions> options)
{
    private static readonly PathString AuthV1Prefix = new("/api/v1/auth");
    private readonly AuthResponseTimingOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(AuthV1Prefix) ||
            !HttpMethods.IsPost(context.Request.Method))
        {
            await next(context);
            return;
        }

        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            var minimum = Math.Clamp(_options.MinimumPublicResponseMilliseconds, 0, 2000);
            var elapsed = Stopwatch.GetElapsedTime(started);
            var remaining = TimeSpan.FromMilliseconds(minimum) - elapsed;
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining, context.RequestAborted);
        }
    }
}
