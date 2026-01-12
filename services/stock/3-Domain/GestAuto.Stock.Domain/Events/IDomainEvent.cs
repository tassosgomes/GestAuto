namespace GestAuto.Stock.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
