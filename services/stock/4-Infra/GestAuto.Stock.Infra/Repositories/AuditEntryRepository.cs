using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.Infra.Repositories;

public sealed class AuditEntryRepository : IAuditEntryRepository
{
    private readonly StockDbContext _context;

    public AuditEntryRepository(StockDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditEntry>> ListByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEntries
            .AsNoTracking()
            .Where(a => a.VehicleId == vehicleId)
            .OrderBy(a => a.OccurredAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        return _context.AuditEntries.AddAsync(entry, cancellationToken).AsTask();
    }
}
