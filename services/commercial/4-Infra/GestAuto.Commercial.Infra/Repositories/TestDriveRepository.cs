using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra.Repositories;

public class TestDriveRepository : ITestDriveRepository
{
    private readonly CommercialDbContext _context;

    public TestDriveRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task<TestDrive?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TestDrives
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TestDrive>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        return await _context.TestDrives
            .Where(t => t.LeadId == leadId)
            .OrderByDescending(t => t.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TestDrive> AddAsync(TestDrive testDrive, CancellationToken cancellationToken = default)
    {
        await _context.TestDrives.AddAsync(testDrive, cancellationToken);
        return testDrive;
    }

    public Task UpdateAsync(TestDrive testDrive, CancellationToken cancellationToken = default)
    {
        _context.TestDrives.Update(testDrive);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<TestDrive>> GetBySalesPersonAsync(
        Guid salesPersonId, 
        DateTime? fromDate, 
        DateTime? toDate, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.TestDrives
            .Where(t => t.ScheduledBy == salesPersonId);

        if (fromDate.HasValue)
            query = query.Where(t => t.ScheduledAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.ScheduledAt <= toDate.Value);

        return await query
            .OrderBy(t => t.ScheduledAt)
            .ToListAsync(cancellationToken);
    }
}