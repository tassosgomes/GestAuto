using GestAuto.Stock.Domain.History;

namespace GestAuto.Stock.Domain.Events;

public record VehicleTestDriveCompletedEvent(
    Guid TestDriveId,
    Guid VehicleId,
    Guid CompletedByUserId,
    DateTime CompletedAtUtc,
    TestDriveOutcome Outcome) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
