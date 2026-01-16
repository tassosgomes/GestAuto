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

    public async Task<(IReadOnlyList<Reservation> Items, int Total)> ListAsync(
        int page,
        int size,
        ReservationStatus? status,
        ReservationType? type,
        Guid? salesPersonId,
        Guid? vehicleId,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (size < 1)
        {
            size = 10;
        }

        var reservations = _context.Reservations.AsNoTracking();

        if (status.HasValue)
        {
            reservations = reservations.Where(r => r.Status == status.Value);
        }

        if (type.HasValue)
        {
            reservations = reservations.Where(r => r.Type == type.Value);
        }

        if (salesPersonId.HasValue && salesPersonId.Value != Guid.Empty)
        {
            reservations = reservations.Where(r => r.SalesPersonId == salesPersonId.Value);
        }

        if (vehicleId.HasValue && vehicleId.Value != Guid.Empty)
        {
            reservations = reservations.Where(r => r.VehicleId == vehicleId.Value);
        }

        var total = await reservations.CountAsync(cancellationToken);

        var items = await reservations
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
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
