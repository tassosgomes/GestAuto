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
/// Gerencia leads e interações com clientes
/// </summary>
[ApiController]
[Route("api/v1/leads")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class LeadController : ControllerBase
{
    private readonly ICommandHandler<CreateLeadCommand, LeadResponse> _createLeadHandler;
    private readonly ICommandHandler<QualifyLeadCommand, LeadResponse> _qualifyLeadHandler;
    private readonly ICommandHandler<ChangeLeadStatusCommand, LeadResponse> _changeStatusHandler;
    private readonly ICommandHandler<UpdateLeadCommand, LeadResponse> _updateLeadHandler;
    private readonly ICommandHandler<RegisterInteractionCommand, InteractionResponse> _registerInteractionHandler;
    private readonly IQueryHandler<GetLeadQuery, LeadResponse> _getLeadHandler;
    private readonly IQueryHandler<ListLeadsQuery, PagedResponse<LeadListItemResponse>> _listLeadsHandler;
    private readonly IQueryHandler<ListInteractionsQuery, IReadOnlyList<InteractionResponse>> _listInteractionsHandler;
    private readonly ISalesPersonFilterService _salesPersonFilter;
    private readonly ILogger<LeadController> _logger;

    public LeadController(
        ICommandHandler<CreateLeadCommand, LeadResponse> createLeadHandler,
        ICommandHandler<QualifyLeadCommand, LeadResponse> qualifyLeadHandler,
        ICommandHandler<ChangeLeadStatusCommand, LeadResponse> changeStatusHandler,
        ICommandHandler<UpdateLeadCommand, LeadResponse> updateLeadHandler,
        ICommandHandler<RegisterInteractionCommand, InteractionResponse> registerInteractionHandler,
        IQueryHandler<GetLeadQuery, LeadResponse> getLeadHandler,
        IQueryHandler<ListLeadsQuery, PagedResponse<LeadListItemResponse>> listLeadsHandler,
        IQueryHandler<ListInteractionsQuery, IReadOnlyList<InteractionResponse>> listInteractionsHandler,
        ISalesPersonFilterService salesPersonFilter,
        ILogger<LeadController> logger)
    {
        _createLeadHandler = createLeadHandler;
        _qualifyLeadHandler = qualifyLeadHandler;
        _changeStatusHandler = changeStatusHandler;
        _updateLeadHandler = updateLeadHandler;
        _registerInteractionHandler = registerInteractionHandler;
        _getLeadHandler = getLeadHandler;
        _listLeadsHandler = listLeadsHandler;
        _listInteractionsHandler = listInteractionsHandler;
        _salesPersonFilter = salesPersonFilter;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo lead
    /// </summary>
    /// <param name="request">Dados do lead</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lead criado</returns>
    /// <response code="201">Lead criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LeadResponse>> Create(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId() 
            ?? throw new UnauthorizedException("Vendedor não identificado");

        var command = new CreateLeadCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.Source,
            salesPersonId,
            request.InterestedModel,
            request.InterestedTrim,
            request.InterestedColor
        );

        var result = await _createLeadHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Lead criado: {LeadId} pelo vendedor {SalesPersonId}", result.Id, salesPersonId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista leads com paginação e filtros
    /// </summary>
    /// <param name="status">Filtro por status (novo, em_contato, em_negociacao, test_drive_agendado, proposta_enviada, perdido, convertido)</param>
    /// <param name="score">Filtro por classificação (hot, warm, cold)</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de leads</returns>
    /// <response code="200">Leads retornados com sucesso</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<LeadListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<LeadListItemResponse>>> List(
        [FromQuery] string? status,
        [FromQuery] string? score,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId();

        var query = new ListLeadsQuery(salesPersonId, status, score, page, pageSize);
        var result = await _listLeadsHandler.HandleAsync(query, cancellationToken);

        _logger.LogInformation("Leads listados: {Count} itens na página {Page}", result.Items.Count, page);

        return Ok(result);
    }

    /// <summary>
    /// Obtém um lead por ID
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do lead</returns>
    /// <response code="200">Lead encontrado</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LeadResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId();

        var query = new GetLeadQuery(id, salesPersonId);
        var result = await _getLeadHandler.HandleAsync(query, cancellationToken);

        _logger.LogInformation("Lead recuperado: {LeadId}", id);

        return Ok(result);
    }

    /// <summary>
    /// Atualiza um lead
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="request">Dados atualizados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lead atualizado</returns>
    /// <response code="200">Lead atualizado com sucesso</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="400">Dados inválidos</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadResponse>> Update(
        Guid id,
        [FromBody] UpdateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLeadCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.InterestedModel,
            request.InterestedTrim,
            request.InterestedColor
        );

        var result = await _updateLeadHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Lead atualizado: {LeadId}", id);

        return Ok(result);
    }

    /// <summary>
    /// Altera o status de um lead
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="request">Novo status</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lead com status atualizado</returns>
    /// <response code="200">Status alterado com sucesso</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="400">Status inválido</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadResponse>> ChangeStatus(
        Guid id,
        [FromBody] ChangeLeadStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeLeadStatusCommand(id, request.Status);
        var result = await _changeStatusHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Status do lead alterado: {LeadId} para {Status}", id, request.Status);

        return Ok(result);
    }

    /// <summary>
    /// Qualifica um lead
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="request">Dados de qualificação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lead qualificado com score calculado</returns>
    /// <response code="200">Lead qualificado com sucesso</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="400">Dados de qualificação inválidos</response>
    [HttpPost("{id:guid}/qualify")]
    [ProducesResponseType(typeof(LeadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadResponse>> Qualify(
        Guid id,
        [FromBody] QualifyLeadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new QualifyLeadCommand(
            id,
            request.HasTradeInVehicle,
            request.TradeInVehicle,
            request.PaymentMethod,
            request.ExpectedPurchaseDate,
            request.InterestedInTestDrive
        );

        var result = await _qualifyLeadHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Lead qualificado: {LeadId} com score {Score}", id, result.Score);

        return Ok(result);
    }

    /// <summary>
    /// Registra uma interação com o lead
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="request">Dados da interação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Interação registrada</returns>
    /// <response code="201">Interação registrada com sucesso</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="400">Dados da interação inválidos</response>
    [HttpPost("{id:guid}/interactions")]
    [ProducesResponseType(typeof(InteractionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InteractionResponse>> RegisterInteraction(
        Guid id,
        [FromBody] RegisterInteractionRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var command = new RegisterInteractionCommand(
            id,
            request.Type,
            request.Description,
            now
        );

        var result = await _registerInteractionHandler.HandleAsync(command, cancellationToken);

        _logger.LogInformation("Interação registrada para o lead: {LeadId}", id);

        return CreatedAtAction(nameof(ListInteractions), new { id }, result);
    }

    /// <summary>
    /// Lista interações de um lead
    /// </summary>
    /// <param name="id">ID do lead</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de interações</returns>
    /// <response code="200">Interações retornadas com sucesso</response>
    /// <response code="404">Lead não encontrado</response>
    [HttpGet("{id:guid}/interactions")]
    [ProducesResponseType(typeof(IReadOnlyList<InteractionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<InteractionResponse>>> ListInteractions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new ListInteractionsQuery(id);
        var result = await _listInteractionsHandler.HandleAsync(query, cancellationToken);

        _logger.LogInformation("Interações listadas para o lead: {LeadId} ({Count} itens)", id, result.Count);

        return Ok(result);
    }
}
