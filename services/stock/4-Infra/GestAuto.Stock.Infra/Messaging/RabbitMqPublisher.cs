using System.Text.Json;
using GestAuto.Stock.Domain.Events;
using GestAuto.Stock.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace GestAuto.Stock.Infra.Messaging;

/// <summary>
/// Implementação de publicador de eventos usando RabbitMQ.
/// Publica eventos de domínio para o broker de mensagens via AMQP.
/// </summary>
public sealed class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
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

    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken)
        where T : IDomainEvent
    {
        if (domainEvent is null)
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
                exchange: RabbitMqConfiguration.StockExchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Evento {EventType} publicado. routingKey={RoutingKey} eventId={EventId}",
                domainEvent.GetType().Name,
                routingKey,
                domainEvent.EventId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao publicar evento {EventType}. eventId={EventId}",
                domainEvent.GetType().Name,
                domainEvent.EventId);

            throw;
        }
    }

    private void DeclareExchange()
    {
        _channel.ExchangeDeclare(
            exchange: RabbitMqConfiguration.StockExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);
    }

    private static string GetRoutingKey(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            VehicleCheckedInEvent => RabbitMqConfiguration.RoutingKeys.VehicleCheckedIn,
            VehicleStatusChangedEvent => RabbitMqConfiguration.RoutingKeys.VehicleStatusChanged,
            ReservationCreatedEvent => RabbitMqConfiguration.RoutingKeys.ReservationCreated,
            ReservationCancelledEvent => RabbitMqConfiguration.RoutingKeys.ReservationCancelled,
            ReservationExtendedEvent => RabbitMqConfiguration.RoutingKeys.ReservationExtended,
            ReservationExpiredEvent => RabbitMqConfiguration.RoutingKeys.ReservationExpired,
            VehicleSoldEvent => RabbitMqConfiguration.RoutingKeys.VehicleSold,
            VehicleTestDriveStartedEvent => RabbitMqConfiguration.RoutingKeys.VehicleTestDriveStarted,
            VehicleTestDriveCompletedEvent => RabbitMqConfiguration.RoutingKeys.VehicleTestDriveCompleted,
            VehicleWrittenOffEvent => RabbitMqConfiguration.RoutingKeys.VehicleWrittenOff,

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
                _channel.Close();
            }
        }
        catch
        {
            // Ignore errors during disposal.
        }
        finally
        {
            _channel.Dispose();
        }
    }
}
