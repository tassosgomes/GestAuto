using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Interfaces;
using GestAuto.Stock.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestAuto.Stock.API.Services;

public sealed class ReservationExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpirationService> _logger;

    private readonly int _batchSize;
    private readonly TimeSpan _pollingInterval;

    public ReservationExpirationService(IServiceScopeFactory scopeFactory, ILogger<ReservationExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

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

        var dbContext = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
        var vehicleRepository = scope.ServiceProvider.GetRequiredService<IVehicleRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;

        var dueReservations = await dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAtUtc != null && r.ExpiresAtUtc <= now)
            .OrderBy(r => r.ExpiresAtUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (dueReservations.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Expirando {Count} reservas vencidas", dueReservations.Count);

        foreach (var reservation in dueReservations)
        {
            reservation.Expire(now);

            var vehicle = await vehicleRepository.GetByIdAsync(reservation.VehicleId, cancellationToken);
            if (vehicle is not null && vehicle.CurrentStatus == VehicleStatus.Reserved)
            {
                vehicle.ChangeStatusManually(VehicleStatus.InStock, changedByUserId: reservation.SalesPersonId, reason: "reservation-expired");
                await vehicleRepository.UpdateAsync(vehicle, cancellationToken);
            }

            await reservationRepository.UpdateAsync(reservation, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
