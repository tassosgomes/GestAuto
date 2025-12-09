using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Infra.Repositories;

namespace GestAuto.Commercial.Infra.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    void AddDomainEvent(IDomainEvent domainEvent);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly CommercialDbContext _context;
    private readonly IOutboxRepository _outboxRepository;
    private IDbContextTransaction? _transaction;
    private readonly List<IDomainEvent> _domainEvents = new();

    public UnitOfWork(CommercialDbContext context, IOutboxRepository outboxRepository)
    {
        _context = context;
        _outboxRepository = outboxRepository;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Coletar eventos das entidades rastreadas antes de salvar
        CollectDomainEventsFromEntities();

        // Salvar eventos no outbox antes de commit para garantir atomicidade
        foreach (var domainEvent in _domainEvents)
        {
            await _outboxRepository.AddAsync(domainEvent, cancellationToken);
        }

        // Salvar todas as mudanças incluindo outbox messages
        var result = await _context.SaveChangesAsync(cancellationToken);
        
        // Limpar eventos após salvar com sucesso
        _domainEvents.Clear();
        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started");

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction started");

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    private void CollectDomainEventsFromEntities()
    {
        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            _domainEvents.AddRange(entity.DomainEvents);
            entity.ClearEvents();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}