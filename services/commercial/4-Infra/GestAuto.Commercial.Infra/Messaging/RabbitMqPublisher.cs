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
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqPublisher(
        IConnection connection,
        ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _channel = _connection.CreateModel();
        
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
    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken) where T : IDomainEvent
    {
        if (domainEvent == null)
        {
            throw new ArgumentNullException(nameof(domainEvent));
        }

        try
        {
            var routingKey = GetRoutingKey(domainEvent);
            var body = JsonSerializer.SerializeToUtf8Bytes(domainEvent, _jsonOptions);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = domainEvent.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = domainEvent.GetType().Name;

            _channel.BasicPublish(
                exchange: RabbitMqConfiguration.CommercialExchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Evento {EventType} publicado com sucesso. RoutingKey: {RoutingKey}, MessageId: {MessageId}",
                domainEvent.GetType().Name,
                routingKey,
                domainEvent.EventId);

            return Task.CompletedTask;
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
            _channel.ExchangeDeclare(
                exchange: RabbitMqConfiguration.CommercialExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null);

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
        _channel?.Close();
        _channel?.Dispose();
    }
}
