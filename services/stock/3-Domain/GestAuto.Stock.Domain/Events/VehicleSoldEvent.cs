namespace GestAuto.Stock.Domain.Events;

public record VehicleSoldEvent(Guid VehicleId, DateTime SoldAtUtc, Guid ResponsibleUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
