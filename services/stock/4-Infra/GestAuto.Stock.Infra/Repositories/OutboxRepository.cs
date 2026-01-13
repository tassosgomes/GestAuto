using System.Text.Json;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Infra.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Stock.Infra.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetPendingMessagesAsync(int batchSize = 50, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}

public class OutboxRepository : IOutboxRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly StockDbContext _context;

    public OutboxRepository(StockDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().FullName!;
        var payload = JsonSerializer.Serialize(domainEvent, JsonOptions);

        var outboxMessage = new OutboxMessage(eventType, payload);

        return _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken).AsTask();
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

        message?.MarkAsProcessed();
    }

    public async Task MarkAsFailedAsync(Guid messageId, string error, CancellationToken cancellationToken = default)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message is null)
        {
            return;
        }

        message.MarkAsFailed(error);
        // Keep ProcessedAt null so it can be retried.
    }
}
