namespace GestAuto.Commercial.Domain.Events;

public record TestDriveScheduledEvent(Guid TestDriveId, Guid LeadId, DateTime ScheduledAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}