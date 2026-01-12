using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Domain.Events;

public record VehicleStatusChangedEvent(
    Guid VehicleId,
    VehicleStatus PreviousStatus,
    VehicleStatus NewStatus,
    Guid ChangedByUserId,
    string Reason) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
