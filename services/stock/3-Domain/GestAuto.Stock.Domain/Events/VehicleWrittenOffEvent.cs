namespace GestAuto.Stock.Domain.Events;

public record VehicleWrittenOffEvent(Guid VehicleId, DateTime WrittenOffAtUtc, Guid ResponsibleUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
