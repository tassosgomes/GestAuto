namespace GestAuto.Commercial.Domain.Events;

public record TestDriveCompletedEvent(Guid TestDriveId, Guid LeadId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}