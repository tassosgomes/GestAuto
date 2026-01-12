using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Exceptions;

namespace GestAuto.Stock.Domain.History;

public sealed class TestDriveSession : BaseEntity
{
    public Guid VehicleId { get; private set; }
    public Guid SalesPersonId { get; private set; }
    public string? CustomerRef { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public TestDriveOutcome? Outcome { get; private set; }

    private TestDriveSession() { }

    public TestDriveSession(Guid vehicleId, Guid salesPersonId, string? customerRef, DateTime startedAt)
    {
        VehicleId = vehicleId;
        SalesPersonId = salesPersonId;
        CustomerRef = customerRef;
        StartedAt = startedAt;
    }

    public void Complete(DateTime endedAt, TestDriveOutcome outcome)
    {
        if (EndedAt.HasValue)
        {
            throw new DomainException("Test-drive session is already completed.");
        }

        if (endedAt < StartedAt)
        {
            throw new DomainException("Test-drive end time must be after start time.");
        }

        EndedAt = endedAt;
        Outcome = outcome;
        Touch();
    }
}
