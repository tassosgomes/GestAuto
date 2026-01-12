namespace GestAuto.Stock.Domain.Events;

public record ReservationReleasedEvent(Guid ReservationId, Guid VehicleId, Guid ReleasedByUserId, string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
