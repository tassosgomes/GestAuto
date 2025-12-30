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
            .Where(t => t.SalesPersonId == salesPersonId);

        if (fromDate.HasValue)
            query = query.Where(t => t.ScheduledAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.ScheduledAt <= toDate.Value);

        return await query
            .OrderBy(t => t.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CheckVehicleAvailabilityAsync(
        Guid vehicleId, 
        DateTime scheduledAt, 
        TimeSpan duration = default, 
        CancellationToken cancellationToken = default)
    {
        // Default to 1 hour if no duration specified
        if (duration == default)
            duration = TimeSpan.FromHours(1);

        var endTime = scheduledAt.Add(duration);

        // Check if there are any scheduled test drives for this vehicle that overlap with the requested time
        var hasConflict = await _context.TestDrives
            .Where(t => t.VehicleId == vehicleId && t.Status == Domain.Enums.TestDriveStatus.Scheduled)
            .Where(t => t.ScheduledAt < endTime && t.CompletedAt > scheduledAt || (t.CompletedAt == null && t.ScheduledAt < endTime))
            .AnyAsync(cancellationToken);

        return !hasConflict;
    }

    public async Task<IEnumerable<TestDrive>> ListAsync(
        Guid? salesPersonId, 
        Guid? leadId, 
        string? status, 
        DateTime? fromDate, 
        DateTime? toDate, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.TestDrives.AsQueryable();

        if (salesPersonId.HasValue && salesPersonId.Value != Guid.Empty)
            query = query.Where(t => t.SalesPersonId == salesPersonId.Value);

        if (leadId.HasValue && leadId.Value != Guid.Empty)
            query = query.Where(t => t.LeadId == leadId.Value);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<Domain.Enums.TestDriveStatus>(status, ignoreCase: true, out var statusEnum))
                query = query.Where(t => t.Status == statusEnum);
        }

        if (fromDate.HasValue)
            query = query.Where(t => t.ScheduledAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.ScheduledAt <= toDate.Value);

        return await query
            .OrderByDescending(t => t.ScheduledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid? salesPersonId, 
        Guid? leadId, 
        string? status, 
        DateTime? fromDate, 
        DateTime? toDate, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.TestDrives.AsQueryable();

        if (salesPersonId.HasValue && salesPersonId.Value != Guid.Empty)
            query = query.Where(t => t.SalesPersonId == salesPersonId.Value);

        if (leadId.HasValue && leadId.Value != Guid.Empty)
            query = query.Where(t => t.LeadId == leadId.Value);

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<Domain.Enums.TestDriveStatus>(status, ignoreCase: true, out var statusEnum))
                query = query.Where(t => t.Status == statusEnum);
        }

        if (fromDate.HasValue)
            query = query.Where(t => t.ScheduledAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.ScheduledAt <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    // Dashboard methods
    public async Task<int> CountByDateAsync(
        DateOnly date,
        string? salesPersonId,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var endOfDay = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var query = _context.TestDrives
            .Where(t =>
                t.ScheduledAt >= startOfDay &&
                t.ScheduledAt <= endOfDay &&
                t.Status == Domain.Enums.TestDriveStatus.Scheduled);

        if (!string.IsNullOrEmpty(salesPersonId) && Guid.TryParse(salesPersonId, out var salesPersonGuid))
            query = query.Where(t => t.SalesPersonId == salesPersonGuid);

        return await query.CountAsync(cancellationToken);
    }
}