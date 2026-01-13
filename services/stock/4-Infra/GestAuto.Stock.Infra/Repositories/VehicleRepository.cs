using GestAuto.Stock.Domain.Entities;
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
