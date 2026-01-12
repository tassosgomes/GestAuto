namespace GestAuto.Stock.Domain.Events;

public record ReservationCancelledEvent(Guid ReservationId, Guid VehicleId, Guid CancelledByUserId, DateTime CancelledAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
