using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;
using GestAuto.Stock.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestAuto.Stock.Infra;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly StockDbContext _context;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IAuditEntryRepository _auditEntryRepository;

    public UnitOfWork(
        StockDbContext context,
        IOutboxRepository outboxRepository,
        IAuditEntryRepository auditEntryRepository)
    {
        _context = context;
        _outboxRepository = outboxRepository;
        _auditEntryRepository = auditEntryRepository;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is VehicleStatusChangedEvent statusChanged)
            {
                await _auditEntryRepository.AddAsync(
                    new AuditEntry(
                        vehicleId: statusChanged.VehicleId,
                        occurredAtUtc: statusChanged.OccurredAt,
                        responsibleUserId: statusChanged.ChangedByUserId,
                        previousStatus: statusChanged.PreviousStatus,
                        newStatus: statusChanged.NewStatus,
                        reason: statusChanged.Reason),
                    cancellationToken);
            }

            await _outboxRepository.AddAsync(domainEvent, cancellationToken);
        }

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearEvents();
        }

        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsActiveReservationConstraintViolation(ex))
        {
            throw new ConflictException("Vehicle already has an active reservation.");
        }
    }

    private static bool IsActiveReservationConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return string.Equals(pg.ConstraintName, "ux_reservations_vehicle_active", StringComparison.OrdinalIgnoreCase);
        }

        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("ux_reservations_vehicle_active", StringComparison.OrdinalIgnoreCase);
    }
}
