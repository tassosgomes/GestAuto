---
status: pending
parallelizable: true
blocked_by: ["6.0", "8.0", "9.0"]
---

<task_context>
<domain>application/evaluations|infra/consumers</domain>
<type>implementation|integration</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>rabbitmq|database</dependencies>
<unblocks>12.0</unblocks>
</task_context>

# Tarefa 11.0: Implementar Fluxo de Avaliações e Consumers

## Visão Geral

Implementar o fluxo de Avaliações de Seminovos (solicitação, resposta, aceite/recusa) e os Consumers de eventos externos (resposta de avaliação do módulo Seminovos, atualização de pedido do módulo Financeiro). Inclui Application Layer, API Layer e Consumers RabbitMQ.

<requirements>
- Implementar Commands/Queries para Avaliações de Seminovos
- Implementar EvaluationController com endpoints REST
- Implementar Consumer para evento AvaliacaoSeminovoRespondida
- Implementar Consumer para evento PedidoAtualizado
- Garantir processamento idempotente dos eventos
- Atualizar proposta com valor da avaliação automaticamente
</requirements>

## Subtarefas

### Application Layer - Avaliações

- [ ] 11.1 Criar `RequestEvaluationCommand` e `RequestEvaluationHandler`
- [ ] 11.2 Criar `RequestEvaluationValidator`
- [ ] 11.3 Criar `RegisterCustomerResponseCommand` e handler (aceite/recusa)
- [ ] 11.4 Criar `GetEvaluationQuery` e `GetEvaluationHandler`
- [ ] 11.5 Criar `ListEvaluationsQuery` e `ListEvaluationsHandler`

### API Layer - Avaliações

- [ ] 11.6 Implementar `EvaluationController` com endpoint `POST /api/v1/used-vehicle-evaluations`
- [ ] 11.7 Implementar endpoint `GET /api/v1/used-vehicle-evaluations`
- [ ] 11.8 Implementar endpoint `GET /api/v1/used-vehicle-evaluations/{id}`
- [ ] 11.9 Implementar endpoint `POST /api/v1/used-vehicle-evaluations/{id}/customer-response`

### API Layer - Orders

- [ ] 11.10 Implementar `OrderController` com endpoint `GET /api/v1/orders`
- [ ] 11.11 Implementar endpoint `GET /api/v1/orders/{id}`
- [ ] 11.12 Implementar endpoint `POST /api/v1/orders/{id}/notes`

### Consumers

- [ ] 11.13 Implementar `UsedVehicleEvaluationRespondedConsumer`
- [ ] 11.14 Implementar `OrderUpdatedConsumer`
- [ ] 11.15 Configurar queues e bindings no RabbitMQ
- [ ] 11.16 Implementar retry policy e dead letter queue
- [ ] 11.17 Criar testes unitários e de integração

## Sequenciamento

- **Bloqueado por:** 6.0 (API Leads), 8.0 (API Propostas), 9.0 (Outbox/RabbitMQ)
- **Desbloqueia:** 12.0 (Testes de Integração)
- **Paralelizável:** Sim (pode executar junto com 10.0)

## Detalhes de Implementação

### RequestEvaluationCommand

```csharp
public record RequestEvaluationCommand(
    Guid ProposalId,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory,
    Guid RequestedByUserId
) : ICommand<EvaluationResponse>;

public class RequestEvaluationHandler : ICommandHandler<RequestEvaluationCommand, EvaluationResponse>
{
    private readonly IUsedVehicleEvaluationRepository _evaluationRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RequestEvaluationHandler(
        IUsedVehicleEvaluationRepository evaluationRepository,
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _evaluationRepository = evaluationRepository;
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EvaluationResponse> HandleAsync(
        RequestEvaluationCommand command, 
        CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken)
            ?? throw new NotFoundException($"Proposta {command.ProposalId} não encontrada");

        var usedVehicle = UsedVehicle.Create(
            command.Brand,
            command.Model,
            command.Year,
            command.Mileage,
            new LicensePlate(command.LicensePlate),
            command.Color,
            command.GeneralCondition,
            command.HasDealershipServiceHistory
        );

        var evaluation = UsedVehicleEvaluation.Request(
            command.ProposalId,
            usedVehicle,
            command.RequestedByUserId
        );

        await _evaluationRepository.AddAsync(evaluation, cancellationToken);

        // Atualiza status da proposta
        proposal.SetAwaitingEvaluation(evaluation.Id);
        await _proposalRepository.UpdateAsync(proposal, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EvaluationResponse.FromEntity(evaluation);
    }
}
```

### RegisterCustomerResponseCommand

```csharp
public record RegisterCustomerResponseCommand(
    Guid EvaluationId,
    bool Accepted,
    string? RejectionReason
) : ICommand<EvaluationResponse>;

public class RegisterCustomerResponseHandler : ICommandHandler<RegisterCustomerResponseCommand, EvaluationResponse>
{
    private readonly IUsedVehicleEvaluationRepository _evaluationRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerResponseHandler(
        IUsedVehicleEvaluationRepository evaluationRepository,
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _evaluationRepository = evaluationRepository;
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EvaluationResponse> HandleAsync(
        RegisterCustomerResponseCommand command, 
        CancellationToken cancellationToken)
    {
        var evaluation = await _evaluationRepository.GetByIdAsync(command.EvaluationId, cancellationToken)
            ?? throw new NotFoundException($"Avaliação {command.EvaluationId} não encontrada");

        if (evaluation.Status != EvaluationStatus.Completed)
            throw new DomainException("Avaliação ainda não foi respondida pelo setor de seminovos");

        if (command.Accepted)
        {
            evaluation.CustomerAccept();

            // Atualiza proposta com valor da troca
            var proposal = await _proposalRepository.GetByIdAsync(evaluation.ProposalId, cancellationToken);
            if (proposal != null)
            {
                proposal.SetTradeInValue(evaluation.EvaluatedValue!);
                await _proposalRepository.UpdateAsync(proposal, cancellationToken);
            }
        }
        else
        {
            evaluation.CustomerReject(command.RejectionReason);
        }

        await _evaluationRepository.UpdateAsync(evaluation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EvaluationResponse.FromEntity(evaluation);
    }
}
```

### UsedVehicleEvaluationRespondedConsumer

```csharp
public class UsedVehicleEvaluationRespondedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConnection _connection;
    private readonly ILogger<UsedVehicleEvaluationRespondedConsumer> _logger;
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
        var messageId = ea.BasicProperties.MessageId;

        try
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<EvaluationRespondedEvent>(body);

            if (message == null)
            {
                _logger.LogWarning("Received null message, acknowledging");
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            _logger.LogInformation(
                "Processing evaluation response. EvaluationId: {EvaluationId}, Value: {Value}",
                message.EvaluationId,
                message.EvaluatedValue);

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
                _logger.LogWarning("Evaluation {EvaluationId} not found", message.EvaluationId);
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            if (evaluation.Status == EvaluationStatus.Completed)
            {
                _logger.LogInformation("Evaluation {EvaluationId} already processed", message.EvaluationId);
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            // Atualizar avaliação
            evaluation.MarkAsCompleted(new Money(message.EvaluatedValue), message.Notes);
            await evaluationRepository.UpdateAsync(evaluation, cancellationToken);

            // Atualizar status da proposta
            var proposal = await proposalRepository.GetByIdAsync(evaluation.ProposalId, cancellationToken);
            if (proposal != null)
            {
                proposal.EvaluationCompleted();
                await proposalRepository.UpdateAsync(proposal, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _channel!.BasicAck(ea.DeliveryTag, false);

            _logger.LogInformation(
                "Evaluation {EvaluationId} processed successfully. Value: {Value}",
                message.EvaluationId,
                message.EvaluatedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", messageId);
            _channel!.BasicNack(ea.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}

// Evento externo (definido pelo módulo de seminovos)
public record EvaluationRespondedEvent(
    Guid EvaluationId,
    decimal EvaluatedValue,
    string? Notes,
    DateTime RespondedAt
);
```

### OrderUpdatedConsumer

```csharp
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

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
        
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

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
        try
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<OrderUpdatedEvent>(body);

            if (message == null)
            {
                _channel!.BasicAck(ea.DeliveryTag, false);
                return;
            }

            _logger.LogInformation(
                "Processing order update. OrderId: {OrderId}, Status: {Status}",
                message.OrderId,
                message.NewStatus);

            using var scope = _scopeFactory.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var order = await orderRepository.GetByExternalIdAsync(message.OrderId, cancellationToken);
            
            if (order == null)
            {
                // Criar novo order se não existir
                order = Order.Create(
                    message.OrderId,
                    message.ProposalId,
                    Enum.Parse<OrderStatus>(message.NewStatus, ignoreCase: true)
                );
                await orderRepository.AddAsync(order, cancellationToken);
            }
            else
            {
                order.UpdateStatus(
                    Enum.Parse<OrderStatus>(message.NewStatus, ignoreCase: true),
                    message.EstimatedDeliveryDate
                );
                await orderRepository.UpdateAsync(order, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _channel!.BasicAck(ea.DeliveryTag, false);

            _logger.LogInformation("Order {OrderId} updated to status {Status}", message.OrderId, message.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order update");
            _channel!.BasicNack(ea.DeliveryTag, false, true);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        base.Dispose();
    }
}

public record OrderUpdatedEvent(
    Guid OrderId,
    Guid ProposalId,
    string NewStatus,
    DateTime? EstimatedDeliveryDate,
    DateTime UpdatedAt
);
```

### EvaluationController

```csharp
[ApiController]
[Route("api/v1/used-vehicle-evaluations")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class EvaluationController : ControllerBase
{
    private readonly ICommandHandler<RequestEvaluationCommand, EvaluationResponse> _requestHandler;
    private readonly ICommandHandler<RegisterCustomerResponseCommand, EvaluationResponse> _customerResponseHandler;
    private readonly IQueryHandler<GetEvaluationQuery, EvaluationResponse> _getHandler;
    private readonly IQueryHandler<ListEvaluationsQuery, PagedResponse<EvaluationListItemResponse>> _listHandler;

    // ... constructor ...

    /// <summary>
    /// Solicita avaliação de seminovo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<EvaluationResponse>> Request(
        [FromBody] RequestEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new RequestEvaluationCommand(
            request.ProposalId,
            request.Brand,
            request.Model,
            request.Year,
            request.Mileage,
            request.LicensePlate,
            request.Color,
            request.GeneralCondition,
            request.HasDealershipServiceHistory,
            userId
        );

        var result = await _requestHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista avaliações
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EvaluationListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<EvaluationListItemResponse>>> List(
        [FromQuery] Guid? proposalId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new ListEvaluationsQuery(proposalId, status, page, pageSize);
        var result = await _listHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Obtém uma avaliação por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EvaluationResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetEvaluationQuery(id);
        var result = await _getHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registra aceite ou recusa do cliente sobre o valor da avaliação
    /// </summary>
    [HttpPost("{id:guid}/customer-response")]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EvaluationResponse>> CustomerResponse(
        Guid id,
        [FromBody] CustomerResponseRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerResponseCommand(
            id,
            request.Accepted,
            request.RejectionReason
        );

        var result = await _customerResponseHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
```

### DTOs de Avaliação

```csharp
public record RequestEvaluationRequest(
    Guid ProposalId,
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory
);

public record CustomerResponseRequest(
    bool Accepted,
    string? RejectionReason
);

public record EvaluationResponse(
    Guid Id,
    Guid ProposalId,
    string Status,
    UsedVehicleResponse Vehicle,
    decimal? EvaluatedValue,
    string? EvaluationNotes,
    DateTime RequestedAt,
    DateTime? RespondedAt,
    bool? CustomerAccepted,
    string? CustomerRejectionReason
)
{
    public static EvaluationResponse FromEntity(UsedVehicleEvaluation evaluation) => new(
        evaluation.Id,
        evaluation.ProposalId,
        evaluation.Status.ToString(),
        UsedVehicleResponse.FromEntity(evaluation.Vehicle),
        evaluation.EvaluatedValue?.Amount,
        evaluation.EvaluationNotes,
        evaluation.RequestedAt,
        evaluation.RespondedAt,
        evaluation.CustomerAccepted,
        evaluation.CustomerRejectionReason
    );
}

public record UsedVehicleResponse(
    string Brand,
    string Model,
    int Year,
    int Mileage,
    string LicensePlate,
    string Color,
    string GeneralCondition,
    bool HasDealershipServiceHistory
)
{
    public static UsedVehicleResponse FromEntity(UsedVehicle vehicle) => new(
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.Mileage,
        vehicle.LicensePlate.Formatted,
        vehicle.Color,
        vehicle.GeneralCondition,
        vehicle.HasDealershipServiceHistory
    );
}
```

## Critérios de Sucesso

- [ ] Solicitar avaliação emite evento UsedVehicleEvaluationRequested
- [ ] Consumer processa evento de resposta do módulo Seminovos
- [ ] Avaliação é atualizada com valor e status
- [ ] Proposta é atualizada quando cliente aceita valor
- [ ] Consumer de Order atualiza status do pedido
- [ ] Processamento é idempotente (reprocessamento não causa duplicação)
- [ ] Dead Letter Queue recebe mensagens com falha
- [ ] Logs estruturados registram todas as operações
- [ ] Testes de integração com Testcontainers passam
