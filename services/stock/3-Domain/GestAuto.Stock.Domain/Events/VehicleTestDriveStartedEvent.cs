namespace GestAuto.Stock.Domain.Events;

public record VehicleTestDriveStartedEvent(Guid TestDriveId, Guid VehicleId, Guid SalesPersonId, DateTime StartedAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
