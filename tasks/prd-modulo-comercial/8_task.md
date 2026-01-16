---
status: completed
parallelizable: false
blocked_by: ["7.0"]
---

<task_context>
<domain>api/proposals</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server|authentication</dependencies>
<unblocks>10.0, 11.0</unblocks>
</task_context>

# Tarefa 8.0: Implementar API Layer - Propostas Controller

## Visão Geral

Implementar o ProposalController com todos os endpoints REST para gerenciamento de propostas comerciais. Inclui autorização específica para aprovação de desconto (somente gerente), e documentação OpenAPI.

<requirements>
- Implementar todos os endpoints de Propostas conforme especificação
- Restringir endpoint de aprovação de desconto para gerentes
- Documentar endpoints com Swagger/OpenAPI
- Implementar tratamento de erros padronizado
</requirements>

## Subtarefas

- [ ] 8.1 Implementar `ProposalController` com endpoint `POST /api/v1/proposals`
- [ ] 8.2 Implementar endpoint `GET /api/v1/proposals` (lista paginada)
- [ ] 8.3 Implementar endpoint `GET /api/v1/proposals/{id}`
- [ ] 8.4 Implementar endpoint `PUT /api/v1/proposals/{id}`
- [ ] 8.5 Implementar endpoint `POST /api/v1/proposals/{id}/items`
- [ ] 8.6 Implementar endpoint `DELETE /api/v1/proposals/{id}/items/{itemId}`
- [ ] 8.7 Implementar endpoint `POST /api/v1/proposals/{id}/discount`
- [ ] 8.8 Implementar endpoint `POST /api/v1/proposals/{id}/approve-discount` (somente gerente)
- [ ] 8.9 Implementar endpoint `POST /api/v1/proposals/{id}/close`
- [ ] 8.10 Configurar autorização [Authorize(Policy = "Manager")] no approve-discount
- [ ] 8.11 Documentar endpoints com Swagger
- [ ] 8.12 Criar testes de integração

## Sequenciamento

- **Bloqueado por:** 7.0 (Application Layer Propostas)
- **Desbloqueia:** 10.0 (Test-Drives), 11.0 (Avaliações)
- **Paralelizável:** Não

## Detalhes de Implementação

### ProposalController

```csharp
[ApiController]
[Route("api/v1/proposals")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class ProposalController : ControllerBase
{
    private readonly ICommandHandler<CreateProposalCommand, ProposalResponse> _createHandler;
    private readonly ICommandHandler<UpdateProposalCommand, ProposalResponse> _updateHandler;
    private readonly ICommandHandler<AddProposalItemCommand, ProposalResponse> _addItemHandler;
    private readonly ICommandHandler<RemoveProposalItemCommand, ProposalResponse> _removeItemHandler;
    private readonly ICommandHandler<ApplyDiscountCommand, ProposalResponse> _applyDiscountHandler;
    private readonly ICommandHandler<ApproveDiscountCommand, ProposalResponse> _approveDiscountHandler;
    private readonly ICommandHandler<CloseProposalCommand, ProposalResponse> _closeHandler;
    private readonly IQueryHandler<GetProposalQuery, ProposalResponse> _getHandler;
    private readonly IQueryHandler<ListProposalsQuery, PagedResponse<ProposalListItemResponse>> _listHandler;
    private readonly ISalesPersonFilterService _salesPersonFilter;

    public ProposalController(
        ICommandHandler<CreateProposalCommand, ProposalResponse> createHandler,
        ICommandHandler<UpdateProposalCommand, ProposalResponse> updateHandler,
        ICommandHandler<AddProposalItemCommand, ProposalResponse> addItemHandler,
        ICommandHandler<RemoveProposalItemCommand, ProposalResponse> removeItemHandler,
        ICommandHandler<ApplyDiscountCommand, ProposalResponse> applyDiscountHandler,
        ICommandHandler<ApproveDiscountCommand, ProposalResponse> approveDiscountHandler,
        ICommandHandler<CloseProposalCommand, ProposalResponse> closeHandler,
        IQueryHandler<GetProposalQuery, ProposalResponse> getHandler,
        IQueryHandler<ListProposalsQuery, PagedResponse<ProposalListItemResponse>> listHandler,
        ISalesPersonFilterService salesPersonFilter)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _addItemHandler = addItemHandler;
        _removeItemHandler = removeItemHandler;
        _applyDiscountHandler = applyDiscountHandler;
        _approveDiscountHandler = approveDiscountHandler;
        _closeHandler = closeHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _salesPersonFilter = salesPersonFilter;
    }

    /// <summary>
    /// Cria uma nova proposta comercial
    /// </summary>
    /// <param name="request">Dados da proposta</param>
    /// <returns>Proposta criada</returns>
    /// <response code="201">Proposta criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Lead não encontrado</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProposalResponse>> Create(
        [FromBody] CreateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProposalCommand(
            request.LeadId,
            request.VehicleModel,
            request.VehicleTrim,
            request.VehicleColor,
            request.VehicleYear,
            request.IsReadyDelivery,
            request.VehiclePrice,
            request.PaymentMethod,
            request.DownPayment,
            request.Installments
        );

        var result = await _createHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista propostas com paginação e filtros
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProposalListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProposalListItemResponse>>> List(
        [FromQuery] Guid? leadId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId();

        var query = new ListProposalsQuery(salesPersonId, leadId, status, page, pageSize);
        var result = await _listHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Obtém uma proposta por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProposalResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId();

        var query = new GetProposalQuery(id, salesPersonId);
        var result = await _getHandler.HandleAsync(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Atualiza uma proposta
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProposalResponse>> Update(
        Guid id,
        [FromBody] UpdateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProposalCommand(
            id,
            request.VehicleModel,
            request.VehicleTrim,
            request.VehicleColor,
            request.VehicleYear,
            request.IsReadyDelivery,
            request.VehiclePrice,
            request.PaymentMethod,
            request.DownPayment,
            request.Installments
        );

        var result = await _updateHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adiciona um item extra à proposta (acessório, película, etc.)
    /// </summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProposalResponse>> AddItem(
        Guid id,
        [FromBody] AddProposalItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddProposalItemCommand(id, request.Description, request.Value);
        var result = await _addItemHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    /// <summary>
    /// Remove um item da proposta
    /// </summary>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProposalResponse>> RemoveItem(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveProposalItemCommand(id, itemId);
        var result = await _removeItemHandler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Aplica desconto na proposta
    /// </summary>
    /// <remarks>
    /// Descontos acima de 5% do valor do veículo requerem aprovação gerencial.
    /// Neste caso, o status da proposta mudará para "AwaitingDiscountApproval".
    /// </remarks>
    [HttpPost("{id:guid}/discount")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProposalResponse>> ApplyDiscount(
        Guid id,
        [FromBody] ApplyDiscountRequest request,
        CancellationToken cancellationToken)
    {
        var salesPersonId = GetCurrentUserId();

        var command = new ApplyDiscountCommand(id, request.Amount, request.Reason, salesPersonId);
        var result = await _applyDiscountHandler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Aprova desconto pendente (somente gerente)
    /// </summary>
    /// <remarks>
    /// Este endpoint só pode ser acessado por usuários com role "manager".
    /// </remarks>
    [HttpPost("{id:guid}/approve-discount")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProposalResponse>> ApproveDiscount(
        Guid id,
        CancellationToken cancellationToken)
    {
        var managerId = GetCurrentUserId();

        var command = new ApproveDiscountCommand(id, managerId);
        var result = await _approveDiscountHandler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Fecha a proposta (venda concluída)
    /// </summary>
    /// <remarks>
    /// Ao fechar a proposta:
    /// - O status muda para "Closed"
    /// - O lead associado é marcado como "Converted"
    /// - Um evento "VendaFechada" é emitido para o módulo financeiro
    /// - A proposta não pode mais ser alterada
    /// </remarks>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProposalResponse>> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var salesPersonId = GetCurrentUserId();

        var command = new CloseProposalCommand(id, salesPersonId);
        var result = await _closeHandler.HandleAsync(command, cancellationToken);

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
```

### Request DTOs

```csharp
public record CreateProposalRequest(
    Guid LeadId,
    string VehicleModel,
    string VehicleTrim,
    string VehicleColor,
    int VehicleYear,
    bool IsReadyDelivery,
    decimal VehiclePrice,
    string PaymentMethod,
    decimal? DownPayment,
    int? Installments
);

public record UpdateProposalRequest(
    string VehicleModel,
    string VehicleTrim,
    string VehicleColor,
    int VehicleYear,
    bool IsReadyDelivery,
    decimal VehiclePrice,
    string PaymentMethod,
    decimal? DownPayment,
    int? Installments
);

public record AddProposalItemRequest(
    string Description,
    decimal Value
);

public record ApplyDiscountRequest(
    decimal Amount,
    string Reason
);
```

### Swagger Annotations e Exemplos

```csharp
// Configuração adicional no Program.cs para exemplos
builder.Services.AddSwaggerGen(c =>
{
    // ... configuração existente ...

    c.ExampleFilters();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateProposalRequestExample>();

// Exemplos
public class CreateProposalRequestExample : IExamplesProvider<CreateProposalRequest>
{
    public CreateProposalRequest GetExamples()
    {
        return new CreateProposalRequest(
            LeadId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            VehicleModel: "Corolla",
            VehicleTrim: "XEi 2.0 Flex",
            VehicleColor: "Prata",
            VehicleYear: 2025,
            IsReadyDelivery: true,
            VehiclePrice: 165000.00m,
            PaymentMethod: "Financing",
            DownPayment: 50000.00m,
            Installments: 48
        );
    }
}

public class ProposalResponseExample : IExamplesProvider<ProposalResponse>
{
    public ProposalResponse GetExamples()
    {
        return new ProposalResponse(
            Id: Guid.Parse("660e8400-e29b-41d4-a716-446655440001"),
            LeadId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Status: "InNegotiation",
            VehicleModel: "Corolla",
            VehicleTrim: "XEi 2.0 Flex",
            VehicleColor: "Prata",
            VehicleYear: 2025,
            IsReadyDelivery: true,
            VehiclePrice: 165000.00m,
            DiscountAmount: 5000.00m,
            DiscountReason: "Fidelização de cliente",
            DiscountApproved: false,
            DiscountApproverId: null,
            TradeInValue: 45000.00m,
            PaymentMethod: "Financing",
            DownPayment: 50000.00m,
            Installments: 48,
            Items: new List<ProposalItemResponse>
            {
                new(Guid.NewGuid(), "Película Fumê", 800.00m),
                new(Guid.NewGuid(), "Tapete Borracha", 350.00m)
            },
            UsedVehicleEvaluationId: Guid.Parse("770e8400-e29b-41d4-a716-446655440002"),
            TotalValue: 116150.00m,
            CreatedAt: DateTime.UtcNow.AddDays(-2),
            UpdatedAt: DateTime.UtcNow
        );
    }
}
```

## Critérios de Sucesso

- [ ] Todos os endpoints respondem conforme especificação
- [ ] Endpoint approve-discount restrito a gerentes (403 para vendedores)
- [ ] Criar proposta atualiza status do lead
- [ ] Fechar proposta marca lead como convertido
- [ ] Desconto > 5% muda status da proposta
- [ ] Proposta fechada retorna erro ao tentar modificar
- [ ] Swagger documenta todos os endpoints com exemplos
- [ ] Testes de integração cobrem fluxo completo de proposta
- [ ] Códigos HTTP corretos em todas as situações
