using GestAuto.Stock.Infra;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GestAuto.Stock.API.Services;

public sealed class StockDbHealthCheck : IHealthCheck
{
    private readonly StockDbContext _dbContext;

    public StockDbHealthCheck(StockDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database reachable")
                : HealthCheckResult.Unhealthy("Database not reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check failed", ex);
        }
    }
}
