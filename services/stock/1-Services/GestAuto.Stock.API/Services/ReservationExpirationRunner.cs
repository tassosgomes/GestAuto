using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Interfaces;
using GestAuto.Stock.Infra;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.API.Services;

public sealed class ReservationExpirationRunner
{
    private readonly StockDbContext _dbContext;
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationExpirationRunner(
        StockDbContext dbContext,
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ExpireDueReservationsOnceAsync(DateTime utcNow, int batchSize, CancellationToken cancellationToken)
    {
        var now = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        var dueReservations = await _dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAtUtc != null && r.ExpiresAtUtc <= now)
            .OrderBy(r => r.ExpiresAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (dueReservations.Count == 0)
        {
            return 0;
        }

        foreach (var reservation in dueReservations)
        {
            reservation.Expire(now);

            var vehicle = await _vehicleRepository.GetByIdAsync(reservation.VehicleId, cancellationToken);
            if (vehicle is not null && vehicle.CurrentStatus == VehicleStatus.Reserved)
            {
                vehicle.ChangeStatusManually(
                    VehicleStatus.InStock,
                    changedByUserId: reservation.SalesPersonId,
                    reason: "reservation-expired");

                await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
            }

            await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return dueReservations.Count;
    }
}
