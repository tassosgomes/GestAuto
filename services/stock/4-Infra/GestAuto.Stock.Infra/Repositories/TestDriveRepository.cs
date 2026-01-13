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
