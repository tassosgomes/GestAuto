using System.Text.Json;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Domain.Interfaces;
using GestAuto.Stock.Infra;
using GestAuto.Stock.Infra.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestAuto.Stock.API.Services;

/// <summary>
/// Serviço de fundo que processa mensagens do outbox e as publica no RabbitMQ.
/// Implementa o padrão Outbox Pattern para garantia transacional de eventos.
/// </summary>
public sealed class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;

    private readonly int _batchSize;
    private readonly TimeSpan _pollingInterval;
    private readonly int _maxRetries;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public OutboxProcessorService(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _batchSize = 100;
        _pollingInterval = TimeSpan.FromSeconds(5);
        _maxRetries = 3;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxProcessorService iniciado. batchSize={BatchSize} pollingIntervalSeconds={PollingIntervalSeconds} maxRetries={MaxRetries}",
            _batchSize,
            _pollingInterval.TotalSeconds,
            _maxRetries);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar outbox. Continuando...");
            }

            try
            {
                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("OutboxProcessorService parado");
    }

    private async Task ProcessOutboxAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<StockDbContext>();

        var messages = await outboxRepository.GetPendingMessagesAsync(_batchSize, cancellationToken);
        if (messages.Count == 0)
        {
            return;
        }

        // Only resolve publisher if we actually have work (keeps RabbitMQ connection lazy).
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        _logger.LogDebug("Processando {MessageCount} mensagens do outbox", messages.Count);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, outboxRepository, eventPublisher, dbContext, cancellationToken);
        }
    }

    private async Task ProcessMessageAsync(
        GestAuto.Stock.Infra.Entities.OutboxMessage message,
        IOutboxRepository outboxRepository,
        IEventPublisher eventPublisher,
        StockDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var eventType = Type.GetType(message.EventType, throwOnError: false);
            if (eventType is null)
            {
                _logger.LogWarning(
                    "Tipo de evento não encontrado. eventType={EventType} outboxMessageId={OutboxMessageId}",
                    message.EventType,
                    message.Id);

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    $"Tipo de evento não encontrado: {message.EventType}",
                    cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                return;
            }

            var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType, JsonOptions) as IDomainEvent;
            if (domainEvent is null)
            {
                _logger.LogWarning(
                    "Erro ao desserializar evento. eventType={EventType} outboxMessageId={OutboxMessageId}",
                    message.EventType,
                    message.Id);

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    "Erro ao desserializar evento",
                    cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                return;
            }

            await PublishEventWithRetryAsync(domainEvent, eventPublisher, outboxRepository, dbContext, message.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro inesperado ao processar mensagem do outbox. outboxMessageId={OutboxMessageId}",
                message.Id);

            await outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task PublishEventWithRetryAsync(
        IDomainEvent domainEvent,
        IEventPublisher eventPublisher,
        IOutboxRepository outboxRepository,
        StockDbContext dbContext,
        Guid outboxMessageId,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await eventPublisher.PublishAsync(domainEvent, cancellationToken);
                await outboxRepository.MarkAsProcessedAsync(outboxMessageId, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogDebug(
                    "Mensagem processada com sucesso. outboxMessageId={OutboxMessageId} eventId={EventId} eventType={EventType}",
                    outboxMessageId,
                    domainEvent.EventId,
                    domainEvent.GetType().Name);

                return;
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));

                _logger.LogWarning(
                    ex,
                    "Erro ao publicar evento (tentativa {Attempt}/{MaxRetries}). outboxMessageId={OutboxMessageId} eventId={EventId} eventType={EventType} retryInSeconds={RetryInSeconds}",
                    attempt,
                    _maxRetries,
                    outboxMessageId,
                    domainEvent.EventId,
                    domainEvent.GetType().Name,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha permanente ao publicar evento. outboxMessageId={OutboxMessageId} eventId={EventId} eventType={EventType}",
                    outboxMessageId,
                    domainEvent.EventId,
                    domainEvent.GetType().Name);

                await outboxRepository.MarkAsFailedAsync(
                    outboxMessageId,
                    $"Falha após {_maxRetries} tentativas: {ex.Message}",
                    cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                return;
            }
        }
    }
}
