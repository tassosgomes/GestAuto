using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Domain.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Reservation?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reservation>> ListByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Reservation> Items, int Total)> ListAsync(
        int page,
        int size,
        ReservationStatus? status,
        ReservationType? type,
        Guid? salesPersonId,
        Guid? vehicleId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
