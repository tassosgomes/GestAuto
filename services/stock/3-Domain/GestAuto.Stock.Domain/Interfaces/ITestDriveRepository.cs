using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.Domain.Interfaces;

public interface ITestDriveRepository
{
    Task<TestDriveSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TestDriveSession> Items, int Total)> ListAsync(
        int page,
        int size,
        bool? completed,
        Guid? vehicleId,
        Guid? salesPersonId,
        string? customerRef,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task AddAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default);
    Task UpdateAsync(TestDriveSession testDrive, CancellationToken cancellationToken = default);
}
