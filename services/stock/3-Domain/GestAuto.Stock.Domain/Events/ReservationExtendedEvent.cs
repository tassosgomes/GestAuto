namespace GestAuto.Stock.Domain.Events;

public record ReservationExtendedEvent(Guid ReservationId, Guid VehicleId, Guid ExtendedByUserId, DateTime NewExpiresAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
