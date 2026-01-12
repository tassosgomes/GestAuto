namespace GestAuto.Stock.Domain.Events;

public record ReservationExpiredEvent(Guid ReservationId, Guid VehicleId, DateTime ExpiredAtUtc) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
