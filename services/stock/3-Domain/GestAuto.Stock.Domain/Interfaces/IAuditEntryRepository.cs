using GestAuto.Stock.Domain.Entities;

namespace GestAuto.Stock.Domain.Interfaces;

public interface IAuditEntryRepository
{
    Task<IReadOnlyList<AuditEntry>> ListByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
