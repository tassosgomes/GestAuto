using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Collections.Generic;
using GestAuto.Commercial.Domain.Interfaces;
using GestAuto.Commercial.Domain.ValueObjects;
using GestAuto.Commercial.Infra.UnitOfWork;

namespace GestAuto.Commercial.Infra.Messaging.Consumers;

/// <summary>
/// Consumer para eventos de resposta de avaliação do módulo de seminovos
/// </summary>
public class UsedVehicleEvaluationRespondedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly ILogger<UsedVehicleEvaluationRespondedConsumer> _logger;
    private readonly Dictionary<string, int> _retryCounts = new();
    private IModel? _channel;

    private const string QueueName = "commercial.evaluation-responded";
    private const string ExchangeName = "used-vehicles";
    private const string RoutingKey = "evaluation.responded";

    public UsedVehicleEvaluationRespondedConsumer(
        IServiceScopeFactory scopeFactory,
        IConnection connection,
        ILogger<UsedVehicleEvaluationRespondedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();

        // Declarar exchange (deve existir no módulo de seminovos)
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
                { "x-dead-letter-routing-key", "evaluation-responded.failed" }
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
            var message = JsonSerializer.Deserialize<EvaluationRespondedEvent>(body);

            if (message == null)
            {
                _logger.LogWarning("Received null message with ID {MessageId}, acknowledging", messageId);
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            _logger.LogInformation(
                "Processing evaluation response. EvaluationId: {EvaluationId}, Value: {Value}, CorrelationId: {CorrelationId}",
                message.EvaluationId,
                message.EvaluatedValue,
                correlationId);

            using var scope = _scopeFactory.CreateScope();
            var evaluationRepository = scope.ServiceProvider
                .GetRequiredService<IUsedVehicleEvaluationRepository>();
            var proposalRepository = scope.ServiceProvider
                .GetRequiredService<IProposalRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Idempotência: verificar se já foi processado
            var evaluation = await evaluationRepository.GetByIdAsync(
                message.EvaluationId, 
                cancellationToken);

            if (evaluation == null)
            {
                _logger.LogWarning("Evaluation {EvaluationId} not found, CorrelationId: {CorrelationId}", 
                    message.EvaluationId, correlationId);
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            // Verificar se já foi processado (idempotência)
            if (evaluation.Status == Domain.Enums.EvaluationStatus.Completed)
            {
                _logger.LogInformation("Evaluation {EvaluationId} already processed, CorrelationId: {CorrelationId}", 
                    message.EvaluationId, correlationId);
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            // Atualizar avaliação
            evaluation.MarkAsCompleted(new Money(message.EvaluatedValue), message.Notes);
            await evaluationRepository.UpdateAsync(evaluation, cancellationToken);

            var proposal = await proposalRepository.GetByIdAsync(evaluation.ProposalId);
            if (proposal != null)
            {
                proposal.ApplyEvaluationResult(new Money(message.EvaluatedValue));
                await proposalRepository.UpdateAsync(proposal);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _channel!.BasicAck(ea.DeliveryTag, false);

            _logger.LogInformation(
                "Evaluation {EvaluationId} processed successfully. Value: {Value}, CorrelationId: {CorrelationId}",
                message.EvaluationId,
                message.EvaluatedValue,
                correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}, CorrelationId: {CorrelationId}", 
                messageId, correlationId);

            var retryKey = messageId ?? ea.DeliveryTag.ToString();
            var attempts = _retryCounts.GetValueOrDefault(retryKey);
            attempts++;
            _retryCounts[retryKey] = attempts;

            if (attempts >= 3)
            {
                _logger.LogWarning("Max retry reached for message {MessageId}, sending to DLQ", messageId);
                _channel!.BasicNack(ea.DeliveryTag, false, false);
                _retryCounts.Remove(retryKey);
            }
            else
            {
                // requeue for retry with backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts)), cancellationToken);
                _channel!.BasicNack(ea.DeliveryTag, false, true);
            }
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
/// Evento externo (definido pelo módulo de seminovos)
/// </summary>
public record EvaluationRespondedEvent(
    Guid EvaluationId,
    decimal EvaluatedValue,
    string? Notes,
    DateTime RespondedAt
);