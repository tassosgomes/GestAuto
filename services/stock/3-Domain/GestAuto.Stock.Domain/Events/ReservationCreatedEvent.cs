namespace GestAuto.Stock.Domain.Events;

public record ReservationCreatedEvent(Guid ReservationId, Guid VehicleId, Guid SalesPersonId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
