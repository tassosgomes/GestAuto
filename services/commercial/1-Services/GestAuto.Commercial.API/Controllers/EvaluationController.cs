using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Application.Queries;

namespace GestAuto.Commercial.API.Controllers;

/// <summary>
/// Gerencia avaliações de seminovos
/// </summary>
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
    private readonly ILogger<EvaluationController> _logger;

    public EvaluationController(
        ICommandHandler<RequestEvaluationCommand, EvaluationResponse> requestHandler,
        ICommandHandler<RegisterCustomerResponseCommand, EvaluationResponse> customerResponseHandler,
        IQueryHandler<GetEvaluationQuery, EvaluationResponse> getHandler,
        IQueryHandler<ListEvaluationsQuery, PagedResponse<EvaluationListItemResponse>> listHandler,
        ILogger<EvaluationController> logger)
    {
        _requestHandler = requestHandler;
        _customerResponseHandler = customerResponseHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _logger = logger;
    }

    /// <summary>
    /// Solicita avaliação de seminovo
    /// </summary>
    /// <param name="request">Dados do seminovo para avaliação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Avaliação criada</returns>
    /// <response code="201">Avaliação solicitada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Proposta não encontrada</response>
    [HttpPost]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EvaluationResponse>> Request(
        [FromBody] RequestEvaluationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Solicitando avaliação para proposta {ProposalId} pelo usuário {UserId}", 
            request.ProposalId, userId);

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
        
        _logger.LogInformation("Avaliação {EvaluationId} solicitada com sucesso", result.Id);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Lista avaliações com filtros opcionais
    /// </summary>
    /// <param name="proposalId">Filtro por ID da proposta</param>
    /// <param name="status">Filtro por status (Requested, Completed, Accepted, Rejected)</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de avaliações</returns>
    /// <response code="200">Lista de avaliações retornada com sucesso</response>
    /// <response code="400">Parâmetros de consulta inválidos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EvaluationListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<EvaluationListItemResponse>>> List(
        [FromQuery(Name = "proposalId")] Guid? proposalId,
        [FromQuery(Name = "status")] string? status,
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = new ListEvaluationsQuery(proposalId, status, page, pageSize);
        var result = await _listHandler.HandleAsync(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Obtém uma avaliação por ID
    /// </summary>
    /// <param name="id">ID da avaliação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes da avaliação</returns>
    /// <response code="200">Avaliação encontrada</response>
    /// <response code="404">Avaliação não encontrada</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
    /// <param name="id">ID da avaliação</param>
    /// <param name="request">Resposta do cliente</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Avaliação atualizada</returns>
    /// <response code="200">Resposta do cliente registrada com sucesso</response>
    /// <response code="400">Dados inválidos ou avaliação ainda não foi completada</response>
    /// <response code="404">Avaliação não encontrada</response>
    [HttpPost("{id:guid}/customer-response")]
    [ProducesResponseType(typeof(EvaluationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EvaluationResponse>> CustomerResponse(
        Guid id,
        [FromBody] CustomerResponseRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registrando resposta do cliente para avaliação {EvaluationId}: {Accepted}", 
            id, request.Accepted);

        var command = new RegisterCustomerResponseCommand(
            id,
            request.Accepted,
            request.RejectionReason
        );

        var result = await _customerResponseHandler.HandleAsync(command, cancellationToken);
        
        _logger.LogInformation("Resposta do cliente registrada para avaliação {EvaluationId}", id);
        
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}