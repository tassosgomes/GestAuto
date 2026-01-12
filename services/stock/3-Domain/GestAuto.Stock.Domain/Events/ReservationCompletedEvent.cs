namespace GestAuto.Stock.Domain.Events;

public record ReservationCompletedEvent(Guid ReservationId, Guid VehicleId, Guid CompletedByUserId, DateTime CompletedAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
