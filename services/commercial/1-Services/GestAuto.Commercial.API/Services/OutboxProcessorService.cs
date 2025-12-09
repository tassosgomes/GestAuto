using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Infra.Repositories;

namespace GestAuto.Commercial.API.Services;

/// <summary>
/// Serviço de fundo que processa mensagens do outbox e as publica no RabbitMQ.
/// Implementa o padrão Outbox Pattern para garantia transacional de eventos.
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;
    
    // Configuração
    private readonly int _batchSize;
    private readonly TimeSpan _pollingInterval;
    private readonly int _maxRetries;

    public OutboxProcessorService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Valores padrão - podem ser configurados via appsettings
        _batchSize = 100;
        _pollingInterval = TimeSpan.FromSeconds(5);
        _maxRetries = 3;
    }

    /// <summary>
    /// Executa o processamento contínuo do outbox.
    /// Faz polling periódico da tabela de outbox e publica eventos pendentes.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxProcessorService iniciado. BatchSize={BatchSize}, PollingInterval={PollingIntervalSeconds}s, MaxRetries={MaxRetries}",
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
                _logger.LogDebug("OutboxProcessorService cancelado");
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

    /// <summary>
    /// Processa um lote de mensagens pendentes do outbox.
    /// </summary>
    private async Task ProcessOutboxAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        var messages = await outboxRepository.GetPendingMessagesAsync(_batchSize, cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Processando {MessageCount} mensagens do outbox", messages.Count);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, outboxRepository, eventPublisher, cancellationToken);
        }
    }

    /// <summary>
    /// Processa uma mensagem individual do outbox.
    /// Desserializa o evento, o publica no RabbitMQ e marca como processado.
    /// </summary>
    private async Task ProcessMessageAsync(
        GestAuto.Commercial.Infra.Entities.OutboxMessage message,
        IOutboxRepository outboxRepository,
        IEventPublisher eventPublisher,
        CancellationToken cancellationToken)
    {
        try
        {
            // Obter o tipo do evento pelo nome qualificado
            var eventType = Type.GetType(message.EventType);
            if (eventType == null)
            {
                _logger.LogWarning(
                    "Tipo de evento não encontrado: {EventType}. MessageId: {MessageId}",
                    message.EventType,
                    message.Id);

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    $"Tipo de evento não encontrado: {message.EventType}",
                    cancellationToken);

                return;
            }

            // Desserializar o evento
            var domainEvent = JsonSerializer.Deserialize(
                message.Payload,
                eventType,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ) as IDomainEvent;

            if (domainEvent == null)
            {
                _logger.LogWarning(
                    "Erro ao desserializar evento. EventType: {EventType}, MessageId: {MessageId}",
                    message.EventType,
                    message.Id);

                await outboxRepository.MarkAsFailedAsync(
                    message.Id,
                    "Erro ao desserializar evento",
                    cancellationToken);

                return;
            }

            // Publicar evento no RabbitMQ
            await PublishEventWithRetryAsync(
                domainEvent,
                eventPublisher,
                message.Id,
                outboxRepository,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro inesperado ao processar mensagem. MessageId: {MessageId}",
                message.Id);

            await outboxRepository.MarkAsFailedAsync(
                message.Id,
                ex.Message,
                cancellationToken);
        }
    }

    /// <summary>
    /// Publica um evento com lógica de retry simples.
    /// Em caso de falha, a mensagem é marcada como falha mas continua no outbox para reprocessamento.
    /// </summary>
    private async Task PublishEventWithRetryAsync(
        IDomainEvent domainEvent,
        IEventPublisher eventPublisher,
        Guid messageId,
        IOutboxRepository outboxRepository,
        CancellationToken cancellationToken)
    {
        int attempt = 0;

        while (attempt < _maxRetries)
        {
            try
            {
                await eventPublisher.PublishAsync(domainEvent, cancellationToken);

                // Sucesso - marcar como processado
                await outboxRepository.MarkAsProcessedAsync(messageId, cancellationToken);

                _logger.LogDebug(
                    "Mensagem processada com sucesso. EventType: {EventType}, MessageId: {MessageId}",
                    domainEvent.GetType().Name,
                    messageId);

                return;
            }
            catch (Exception ex) when (attempt < _maxRetries - 1)
            {
                attempt++;
                
                _logger.LogWarning(
                    ex,
                    "Erro ao publicar evento (tentativa {Attempt}/{MaxRetries}). EventType: {EventType}, MessageId: {MessageId}",
                    attempt,
                    _maxRetries,
                    domainEvent.GetType().Name,
                    messageId);

                // Backoff exponencial simples: 1s, 2s, 4s
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), cancellationToken);
            }
            catch (Exception ex)
            {
                // Falha após todas as tentativas
                _logger.LogError(
                    ex,
                    "Falha permanente ao publicar evento após {MaxRetries} tentativas. EventType: {EventType}, MessageId: {MessageId}",
                    _maxRetries,
                    domainEvent.GetType().Name,
                    messageId);

                await outboxRepository.MarkAsFailedAsync(
                    messageId,
                    $"Falha após {_maxRetries} tentativas: {ex.Message}",
                    cancellationToken);

                throw;
            }
        }
    }
}
