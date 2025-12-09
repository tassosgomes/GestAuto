using GestAuto.Commercial.Domain.Enums;

namespace GestAuto.Commercial.Domain.Events;

public record LeadScoredEvent(Guid LeadId, LeadScore Score) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}