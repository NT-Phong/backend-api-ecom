using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Ecom.Application.Common.Interfaces;

namespace Ecom.Infrastructure.HealthChecks;

public class ReadinessHealthCheck : IHealthCheck
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<ReadinessHealthCheck> _logger;

    public ReadinessHealthCheck(IApplicationDbContext dbContext, ILogger<ReadinessHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity
            var dbContext = (DbContext)_dbContext;
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogWarning("Database connection failed during readiness check");
                return HealthCheckResult.Unhealthy("Database is not accessible");
            }

            // Additional checks can be added here:
            // - Redis connectivity
            // - External API availability
            // - Required message queue connections
            // - etc.

            return HealthCheckResult.Healthy("All external dependencies are accessible");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return HealthCheckResult.Unhealthy($"Readiness check failed: {ex.Message}");
        }
    }
}
