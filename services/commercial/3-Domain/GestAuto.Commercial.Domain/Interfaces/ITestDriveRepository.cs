using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface ITestDriveRepository
{
    Task<TestDrive?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestDrive>> GetByLeadIdAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task<TestDrive> AddAsync(TestDrive testDrive, CancellationToken cancellationToken = default);
    Task UpdateAsync(TestDrive testDrive, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestDrive>> GetBySalesPersonAsync(Guid salesPersonId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<bool> CheckVehicleAvailabilityAsync(Guid vehicleId, DateTime scheduledAt, TimeSpan duration = default, CancellationToken cancellationToken = default);
    Task<IEnumerable<TestDrive>> ListAsync(Guid? salesPersonId, Guid? leadId, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? salesPersonId, Guid? leadId, string? status, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
}
