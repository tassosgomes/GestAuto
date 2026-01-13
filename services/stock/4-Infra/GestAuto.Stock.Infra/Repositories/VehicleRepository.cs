using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        _context.Vehicles.Update(vehicle);
        return Task.CompletedTask;
    }
}
