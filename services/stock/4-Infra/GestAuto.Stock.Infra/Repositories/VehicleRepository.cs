using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GestAuto.Stock.Infra.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly StockDbContext _context;

    public VehicleRepository(StockDbContext context)
    {
        _context = context;
    }

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles
            .Include(v => v.CheckIns)
            .Include(v => v.CheckOuts)
            .Include(v => v.TestDrives)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles
            .FirstOrDefaultAsync(v => v.Vin == vin, cancellationToken);
    }

    public Task<Vehicle?> GetByPlateAsync(string plate, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles
            .FirstOrDefaultAsync(v => v.Plate == plate, cancellationToken);
    }

    public async Task<(IReadOnlyList<Vehicle> Items, int Total)> ListAsync(
        int page,
        int size,
        VehicleStatus? status,
        VehicleCategory? category,
        string? query,
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

        var vehicles = _context.Vehicles.AsNoTracking();

        if (status.HasValue)
        {
            vehicles = vehicles.Where(v => v.CurrentStatus == status.Value);
        }

        if (category.HasValue)
        {
            vehicles = vehicles.Where(v => v.Category == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();

            vehicles = vehicles.Where(v =>
                EF.Functions.ILike(v.Vin, $"%{q}%") ||
                (v.Plate != null && EF.Functions.ILike(v.Plate, $"%{q}%")) ||
                EF.Functions.ILike(v.Make, $"%{q}%") ||
                EF.Functions.ILike(v.Model, $"%{q}%") ||
                (v.Trim != null && EF.Functions.ILike(v.Trim, $"%{q}%")));
        }

        var total = await vehicles.CountAsync(cancellationToken);

        var items = await vehicles
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        return _context.Vehicles.AddAsync(vehicle, cancellationToken).AsTask();
    }

    public Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        // In this module, command handlers usually load the aggregate (tracked) and then
        // append new history records (check-ins/check-outs/test-drives) to navigation collections.
        // Some EF configurations can incorrectly treat the newly created record as Modified,
        // producing UPDATEs that affect 0 rows. We force the newest record to be Added.
        EnsureLastHistoryRecordIsAdded(vehicle.CheckIns);
        EnsureLastHistoryRecordIsAdded(vehicle.CheckOuts);
        EnsureLastHistoryRecordIsAdded(vehicle.TestDrives);

        // When the entity is loaded through this repository, it is already tracked.
        // Calling Update(vehicle) would mark the entire aggregate graph as Modified,
        // which can lead to incorrect UPDATEs for newly added child records.
        if (_context.Entry(vehicle).State == EntityState.Detached)
        {
            _context.Vehicles.Update(vehicle);
        }
        return Task.CompletedTask;
    }

    private void EnsureLastHistoryRecordIsAdded<T>(IReadOnlyCollection<T> items)
        where T : class
    {
        if (items.Count == 0)
        {
            return;
        }

        var last = items.Last();
        var entry = _context.Entry(last);

        if (entry.State is EntityState.Detached or EntityState.Modified)
        {
            entry.State = EntityState.Added;
        }
    }
}
