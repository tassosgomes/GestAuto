using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Infra.Entities;

namespace GestAuto.Commercial.Infra.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 50, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}

public class OutboxRepository : IOutboxRepository
{
    private readonly CommercialDbContext _context;

    public OutboxRepository(CommercialDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().FullName!;
        var payload = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        var outboxMessage = new OutboxMessage(eventType, payload);

        await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        return await _context.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        
        if (message != null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            message.Error = null;
        }
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);
        
        if (message != null)
        {
            message.Error = error;
            // Don't set ProcessedAt so it can be retried
        }
    }
}