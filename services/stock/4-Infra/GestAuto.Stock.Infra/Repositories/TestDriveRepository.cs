using GestAuto.Stock.Domain.History;
using GestAuto.Stock.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.Infra.Repositories;

public class TestDriveRepository : ITestDriveRepository
{
    private readonly StockDbContext _context;

    public TestDriveRepository(StockDbContext context)
    {
        _context = context;
    }

    public Task<TestDriveSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.TestDrives
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<TestDriveSession> Items, int Total)> ListAsync(
        int page,
        int size,
        bool? completed,
        Guid? vehicleId,
        Guid? salesPersonId,
        string? customerRef,
        DateTime? from,
        DateTime? to,
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

        var testDrives = _context.TestDrives.AsNoTracking();

        if (completed.HasValue)
        {
            testDrives = completed.Value
                ? testDrives.Where(t => t.EndedAt != null)
                : testDrives.Where(t => t.EndedAt == null);
        }

        if (vehicleId.HasValue && vehicleId.Value != Guid.Empty)
        {
            testDrives = testDrives.Where(t => t.VehicleId == vehicleId.Value);
        }

        if (salesPersonId.HasValue && salesPersonId.Value != Guid.Empty)
        {
            testDrives = testDrives.Where(t => t.SalesPersonId == salesPersonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(customerRef))
        {
            var query = customerRef.Trim();
            testDrives = testDrives.Where(t => t.CustomerRef != null && EF.Functions.ILike(t.CustomerRef, $"%{query}%"));
        }

        if (from.HasValue)
        {
            testDrives = testDrives.Where(t => t.StartedAt >= from.Value);
        }

        if (to.HasValue)
        {
            testDrives = testDrives.Where(t => t.StartedAt <= to.Value);
        }

        var total = await testDrives.CountAsync(cancellationToken);

        var items = await testDrives
            .OrderByDescending(t => t.StartedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public Task AddAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default)
    {
        return _context.TestDrives.AddAsync(testDrive, cancellationToken).AsTask();
    }

    public Task UpdateAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default)
    {
        _context.TestDrives.Update(testDrive);
        return Task.CompletedTask;
    }
}
