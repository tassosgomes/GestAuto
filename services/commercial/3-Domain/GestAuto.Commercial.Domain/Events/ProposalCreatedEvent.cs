namespace GestAuto.Commercial.Domain.Events;

public record ProposalCreatedEvent(Guid ProposalId, Guid LeadId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}