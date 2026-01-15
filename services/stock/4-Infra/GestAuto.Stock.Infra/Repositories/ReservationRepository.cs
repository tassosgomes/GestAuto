using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.Infra.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly StockDbContext _context;

    public ReservationRepository(StockDbContext context)
    {
        _context = context;
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<Reservation?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return _context.Reservations
            .FirstOrDefaultAsync(r => r.VehicleId == vehicleId && r.Status == ReservationStatus.Active, cancellationToken);
    }

    public async Task<IReadOnlyList<Reservation>> ListByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(r => r.VehicleId == vehicleId)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        return _context.Reservations.AddAsync(reservation, cancellationToken).AsTask();
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(reservation);
        return Task.CompletedTask;
    }
}
