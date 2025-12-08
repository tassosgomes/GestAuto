using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Events;

public record LeadStatusChangedEvent(Guid LeadId, LeadStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}