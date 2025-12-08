using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Events;

public record LeadCreatedEvent(Guid LeadId, string Name, LeadSource Source) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}