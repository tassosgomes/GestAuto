namespace GestAuto.Commercial.Domain.Events;

public record ProposalUpdatedEvent(Guid ProposalId, string Description) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}