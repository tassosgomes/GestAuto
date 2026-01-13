using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.Domain.Interfaces;

public interface ITestDriveRepository
{
    Task<TestDriveSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default);
    Task UpdateAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default);
}
