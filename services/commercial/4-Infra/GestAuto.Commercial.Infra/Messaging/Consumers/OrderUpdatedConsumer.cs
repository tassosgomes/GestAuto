using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Infra.Messaging.Consumers;

/// <summary>
/// Consumer para eventos de atualização de pedido do módulo financeiro
/// </summary>
public class OrderUpdatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly ILogger<OrderUpdatedConsumer> _logger;
    private IModel? _channel;

    private const string QueueName = "commercial.order-updated";
    private const string ExchangeName = "finance";
    private const string RoutingKey = "order.updated";

    public OrderUpdatedConsumer(
        IServiceScopeFactory scopeFactory,
        IConnection connection,
        ILogger<OrderUpdatedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();

        // Declarar exchange (deve existir no módulo financeiro)
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        // Declarar queue
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "commercial.dlx" },
                { "x-dead-letter-routing-key", "order-updated.failed" }
            });

        // Bind queue to exchange
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

        // Configure QoS to process one message at a time
        _channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            await ProcessMessageAsync(ea, stoppingToken);
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Consumer started for queue {QueueName}", QueueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        var messageId = ea.BasicProperties.MessageId ?? "unknown";
        var correlationId = ea.BasicProperties.CorrelationId ?? "unknown";

        try
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<OrderUpdatedEvent>(body);

            if (message == null)
            {
                _logger.LogWarning("Received null message with ID {MessageId}, acknowledging", messageId);
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            _logger.LogInformation(
                "Processing order update. OrderId: {OrderId}, Status: {Status}, CorrelationId: {CorrelationId}",
                message.OrderId,
                message.NewStatus,
                correlationId);

            using var scope = _scopeFactory.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Buscar order por ExternalId
            var order = await orderRepository.GetByExternalIdAsync(message.OrderId, cancellationToken);
            
            if (order == null)
            {
                // Criar novo order se não existir
                if (!Enum.TryParse<OrderStatus>(message.NewStatus, ignoreCase: true, out var status))
                {
                    _logger.LogWarning("Invalid order status: {Status}, CorrelationId: {CorrelationId}", 
                        message.NewStatus, correlationId);
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                order = Order.Create(
                    message.OrderId,
                    message.ProposalId,
                    status
                );
                await orderRepository.AddAsync(order, cancellationToken);

                _logger.LogInformation("Created new order with external ID {OrderId}, CorrelationId: {CorrelationId}", 
                    message.OrderId, correlationId);
            }
            else
            {
                // Atualizar order existente
                if (Enum.TryParse<OrderStatus>(message.NewStatus, ignoreCase: true, out var status))
                {
                    order.UpdateStatus(status, message.EstimatedDeliveryDate);
                    await orderRepository.UpdateAsync(order, cancellationToken);

                    _logger.LogInformation("Updated existing order {OrderId} to status {Status}, CorrelationId: {CorrelationId}", 
                        message.OrderId, message.NewStatus, correlationId);
                }
                else
                {
                    _logger.LogWarning("Invalid order status: {Status}, CorrelationId: {CorrelationId}", 
                        message.NewStatus, correlationId);
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _channel!.BasicAck(ea.DeliveryTag, false);

            _logger.LogInformation("Order {OrderId} processed successfully with status {Status}, CorrelationId: {CorrelationId}", 
                message.OrderId, message.NewStatus, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order update message {MessageId}, CorrelationId: {CorrelationId}", 
                messageId, correlationId);
            
            // Rejeitar mensagem e enviá-la para DLQ
            _channel!.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Evento externo (definido pelo módulo financeiro)
/// </summary>
public record OrderUpdatedEvent(
    Guid OrderId,
    Guid ProposalId,
    string NewStatus,
    DateTime? EstimatedDeliveryDate,
    DateTime UpdatedAt
);