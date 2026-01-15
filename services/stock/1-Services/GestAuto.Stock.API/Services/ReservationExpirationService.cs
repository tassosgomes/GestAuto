using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestAuto.Stock.API.Services;

public sealed class ReservationExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpirationService> _logger;
    private readonly TimeProvider _timeProvider;

    private readonly int _batchSize;
    private readonly TimeSpan _pollingInterval;

    public ReservationExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationExpirationService> logger,
        TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;

        _batchSize = 200;
        _pollingInterval = TimeSpan.FromMinutes(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ReservationExpirationService iniciado. batchSize={BatchSize} pollingIntervalSeconds={PollingIntervalSeconds}",
            _batchSize,
            _pollingInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireDueReservationsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao expirar reservas. Continuando...");
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("ReservationExpirationService parado");
    }

    private async Task ExpireDueReservationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<ReservationExpirationRunner>();

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var expired = await runner.ExpireDueReservationsOnceAsync(nowUtc, _batchSize, cancellationToken);

        if (expired > 0)
        {
            _logger.LogInformation("Expiradas {Count} reservas vencidas", expired);
        }
    }
}
