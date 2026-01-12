using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Domain.Events;

public record VehicleCheckedInEvent(Guid VehicleId, CheckInSource Source, DateTime CheckedInAt, Guid ResponsibleUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
