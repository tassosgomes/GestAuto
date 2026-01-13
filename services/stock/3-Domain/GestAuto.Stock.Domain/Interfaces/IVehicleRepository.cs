using GestAuto.Stock.Domain.Entities;

namespace GestAuto.Stock.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByPlateAsync(string plate, CancellationToken cancellationToken = default);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
}
