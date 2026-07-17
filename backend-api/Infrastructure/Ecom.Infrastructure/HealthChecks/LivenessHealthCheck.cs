using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecom.Infrastructure.HealthChecks;

public class LivenessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Liveness check - just verify the application is running
        // This should always return healthy unless the application is completely broken
        return Task.FromResult(HealthCheckResult.Healthy("Application is running"));
    }
}
