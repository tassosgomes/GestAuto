using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.API.Services;
using GestAuto.Commercial.Domain.Exceptions;

namespace GestAuto.Commercial.API.Controllers;

/// <summary>
/// Gerencia propostas comerciais e negociações
/// </summary>
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
    private readonly ILogger<ProposalController> _logger;

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
        ISalesPersonFilterService salesPersonFilter,
        ILogger<ProposalController> logger)
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
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova proposta comercial
    /// </summary>
    /// <param name="request">Dados da proposta</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta criada</returns>
    /// <response code="201">Proposta criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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

        _logger.LogInformation("Proposta criada: {ProposalId} para o lead {LeadId}", result.Id, request.LeadId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista propostas com paginação e filtros
    /// </summary>
    /// <param name="leadId">Filtro por ID do lead (opcional)</param>
    /// <param name="status">Filtro por status da proposta (opcional)</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de propostas</returns>
    /// <response code="200">Propostas retornadas com sucesso</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProposalListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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

        _logger.LogInformation("Propostas listadas: {Count} itens na página {Page}", result.Items.Count, page);

        return Ok(result);
    }

    /// <summary>
    /// Obtém uma proposta por ID
    /// </summary>
    /// <param name="id">ID da proposta</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da proposta</returns>
    /// <response code="200">Proposta encontrada</response>
    /// <response code="404">Proposta não encontrada</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProposalResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProposalQuery(id);
        var result = await _getHandler.HandleAsync(query, cancellationToken);

        _logger.LogInformation("Proposta recuperada: {ProposalId}", id);

        return Ok(result);
    }

    /// <summary>
    /// Atualiza uma proposta
    /// </summary>
    /// <param name="id">ID da proposta</param>
    /// <param name="request">Dados atualizados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta atualizada</returns>
    /// <response code="200">Proposta atualizada com sucesso</response>
    /// <response code="404">Proposta não encontrada</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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

        _logger.LogInformation("Proposta atualizada: {ProposalId}", id);

        return Ok(result);
    }

    /// <summary>
    /// Adiciona um item extra à proposta (acessório, película, etc.)
    /// </summary>
    /// <param name="id">ID da proposta</param>
    /// <param name="request">Dados do item</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta atualizada com novo item</returns>
    /// <response code="201">Item adicionado com sucesso</response>
    /// <response code="404">Proposta não encontrada</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProposalResponse>> AddItem(
        Guid id,
        [FromBody] AddProposalItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddProposalItemCommand(id, request.Description, request.Value);
        var result = await _addItemHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Item adicionado à proposta: {ProposalId}", id);

        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    /// <summary>
    /// Remove um item da proposta
    /// </summary>
    /// <param name="id">ID da proposta</param>
    /// <param name="itemId">ID do item a remover</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta atualizada sem o item</returns>
    /// <response code="200">Item removido com sucesso</response>
    /// <response code="404">Proposta ou item não encontrado</response>
    /// <response code="401">Não autorizado</response>
    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProposalResponse>> RemoveItem(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveProposalItemCommand(id, itemId);
        var result = await _removeItemHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Item removido da proposta: {ProposalId}, ItemId: {ItemId}", id, itemId);

        return Ok(result);
    }

    /// <summary>
    /// Aplica desconto na proposta
    /// </summary>
    /// <remarks>
    /// Descontos acima de 5% do valor do veículo requerem aprovação gerencial.
    /// Neste caso, o status da proposta mudará para "AwaitingDiscountApproval".
    /// </remarks>
    /// <param name="id">ID da proposta</param>
    /// <param name="request">Dados do desconto</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta com desconto aplicado</returns>
    /// <response code="200">Desconto aplicado com sucesso</response>
    /// <response code="404">Proposta não encontrada</response>
    /// <response code="400">Desconto inválido</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("{id:guid}/discount")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProposalResponse>> ApplyDiscount(
        Guid id,
        [FromBody] ApplyDiscountRequest request,
        CancellationToken cancellationToken)
    {
        var salesPersonId = GetCurrentUserId();

        var command = new ApplyDiscountCommand(id, request.Amount, request.Reason, salesPersonId);
        var result = await _applyDiscountHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Desconto aplicado à proposta: {ProposalId}, Valor: {Amount}", id, request.Amount);

        return Ok(result);
    }

    /// <summary>
    /// Aprova desconto pendente (somente gerente)
    /// </summary>
    /// <remarks>
    /// Este endpoint só pode ser acessado por usuários com role "manager".
    /// Desconto pendente é aprovado ou rejeitado pelo gerente.
    /// </remarks>
    /// <param name="id">ID da proposta</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta com desconto aprovado</returns>
    /// <response code="200">Desconto aprovado com sucesso</response>
    /// <response code="404">Proposta não encontrada</response>
    /// <response code="400">Proposta não aguarda aprovação de desconto</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Apenas gerentes podem aprovar descontos</response>
    [HttpPost("{id:guid}/approve-discount")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProposalResponse>> ApproveDiscount(
        Guid id,
        CancellationToken cancellationToken)
    {
        var managerId = GetCurrentUserId();

        var command = new ApproveDiscountCommand(id, managerId);
        var result = await _approveDiscountHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Desconto aprovado para a proposta: {ProposalId} pelo gerente {ManagerId}", id, managerId);

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
    /// <param name="id">ID da proposta</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Proposta fechada</returns>
    /// <response code="200">Proposta fechada com sucesso</response>
    /// <response code="404">Proposta não encontrada</response>
    /// <response code="400">Proposta não pode ser fechada</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ProposalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProposalResponse>> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var salesPersonId = GetCurrentUserId();

        var command = new CloseProposalCommand(id, salesPersonId);
        var result = await _closeHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Proposta fechada: {ProposalId} pelo vendedor {SalesPersonId}", id, salesPersonId);

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
