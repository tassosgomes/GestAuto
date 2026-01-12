using GestAuto.Stock.Domain.Events;

namespace GestAuto.Stock.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearEvents()
    {
        _domainEvents.Clear();
    }

    protected void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
