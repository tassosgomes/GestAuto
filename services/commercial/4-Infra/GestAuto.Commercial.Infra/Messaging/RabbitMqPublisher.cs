using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Infra.Messaging;

/// <summary>
/// Implementação de publicador de eventos usando RabbitMQ.
/// Publica eventos de domínio para o broker de mensagens via AMQP.
/// </summary>
public class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqPublisher(
        IConnection connection,
        ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        DeclareExchange();
    }

    /// <summary>
    /// Publica um evento de domínio no RabbitMQ.
    /// </summary>
    /// <typeparam name="T">Tipo do evento que implementa IDomainEvent</typeparam>
    /// <param name="domainEvent">Evento a ser publicado</param>
    /// <param name="cancellationToken">Token para cancelamento</param>
    /// <returns>Task completada quando o evento é publicado</returns>
    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken) where T : IDomainEvent
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        try
        {
            var routingKey = GetRoutingKey(domainEvent);
            var body = JsonSerializer.SerializeToUtf8Bytes(domainEvent, _jsonOptions);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = domainEvent.EventId.ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Type = domainEvent.GetType().Name
            };

            await _channel.BasicPublishAsync(
                exchange: RabbitMqConfiguration.CommercialExchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken);

            _logger.LogInformation(
                "Evento {EventType} publicado com sucesso. RoutingKey: {RoutingKey}, MessageId: {MessageId}",
                domainEvent.GetType().Name,
                routingKey,
                domainEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao publicar evento {EventType}. MessageId: {MessageId}",
                domainEvent.GetType().Name,
                domainEvent.EventId);

            throw;
        }
    }

    /// <summary>
    /// Declara o exchange de eventos comerciais no RabbitMQ.
    /// Idempotente - pode ser chamado múltiplas vezes sem efeitos colaterais.
    /// </summary>
    private void DeclareExchange()
    {
        try
        {
            _channel.ExchangeDeclareAsync(
                exchange: RabbitMqConfiguration.CommercialExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null).GetAwaiter().GetResult();

            _logger.LogDebug(
                "Exchange {ExchangeName} declarado com sucesso",
                RabbitMqConfiguration.CommercialExchange);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao declarar exchange {ExchangeName}",
                RabbitMqConfiguration.CommercialExchange);

            throw;
        }
    }

    /// <summary>
    /// Mapeia um evento de domínio para seu routing key no RabbitMQ.
    /// Cada tipo de evento tem uma chave de roteamento específica.
    /// </summary>
    /// <param name="domainEvent">Evento de domínio</param>
    /// <returns>Routing key para o evento</returns>
    /// <exception cref="ArgumentException">Se o tipo de evento não for reconhecido</exception>
    private static string GetRoutingKey(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            LeadCreatedEvent => RabbitMqConfiguration.RoutingKeys.LeadCreated,
            LeadScoredEvent => RabbitMqConfiguration.RoutingKeys.LeadScored,
            LeadStatusChangedEvent => RabbitMqConfiguration.RoutingKeys.LeadStatusChanged,
            ProposalCreatedEvent => RabbitMqConfiguration.RoutingKeys.ProposalCreated,
            ProposalUpdatedEvent => RabbitMqConfiguration.RoutingKeys.ProposalUpdated,
            SaleClosedEvent => RabbitMqConfiguration.RoutingKeys.SaleClosed,
            TestDriveScheduledEvent => RabbitMqConfiguration.RoutingKeys.TestDriveScheduled,
            TestDriveCompletedEvent => RabbitMqConfiguration.RoutingKeys.TestDriveCompleted,
            UsedVehicleEvaluationRequestedEvent => RabbitMqConfiguration.RoutingKeys.EvaluationRequested,
            _ => throw new ArgumentException(
                $"Tipo de evento não reconhecido: {domainEvent.GetType().Name}",
                nameof(domainEvent))
        };
    }

    public void Dispose()
    {
        try
        {
            if (_channel.IsOpen)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch
        {
            // Ignore errors during disposal
        }
        finally
        {
            _channel?.Dispose();
        }
    }
}
