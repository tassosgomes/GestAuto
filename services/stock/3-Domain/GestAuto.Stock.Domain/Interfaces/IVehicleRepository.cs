using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByPlateAsync(string plate, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Vehicle> Items, int Total)> ListAsync(
        int page,
        int size,
        VehicleStatus? status,
        VehicleCategory? category,
        string? query,
        CancellationToken cancellationToken = default);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
}
