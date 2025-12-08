using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Events;

public record SaleClosedEvent(Guid ProposalId, Guid LeadId, Money TotalValue) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}