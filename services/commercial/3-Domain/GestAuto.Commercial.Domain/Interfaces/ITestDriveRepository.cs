using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Domain.Interfaces;

public interface ITestDriveRepository
{
    Task<TestDrive?> GetByIdAsync(Guid id);
    Task<IEnumerable<TestDrive>> GetByLeadIdAsync(Guid leadId);
    Task AddAsync(TestDrive testDrive);
    Task UpdateAsync(TestDrive testDrive);
}
