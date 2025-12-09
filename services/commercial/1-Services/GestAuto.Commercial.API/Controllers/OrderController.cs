using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestAuto.Commercial.Application.Commands;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Application.Queries;

namespace GestAuto.Commercial.API.Controllers;

/// <summary>
/// Gerencia pedidos de vendas
/// </summary>
[ApiController]
[Route("api/v1/orders")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private readonly IQueryHandler<GetOrderQuery, OrderResponse> _getOrderHandler;
    private readonly IQueryHandler<ListOrdersQuery, PagedResponse<OrderListItemResponse>> _listOrdersHandler;
    private readonly ICommandHandler<AddOrderNotesCommand, OrderResponse> _addNotesHandler;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        IQueryHandler<GetOrderQuery, OrderResponse> getOrderHandler,
        IQueryHandler<ListOrdersQuery, PagedResponse<OrderListItemResponse>> listOrdersHandler,
        ICommandHandler<AddOrderNotesCommand, OrderResponse> addNotesHandler,
        ILogger<OrderController> logger)
    {
        _getOrderHandler = getOrderHandler;
        _listOrdersHandler = listOrdersHandler;
        _addNotesHandler = addNotesHandler;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os pedidos
    /// </summary>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de pedidos</returns>
    /// <response code="200">Lista de pedidos retornada com sucesso</response>
    /// <response code="400">Parâmetros de consulta inválidos</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<OrderListItemResponse>>> List(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = new ListOrdersQuery(page, pageSize);
        var result = await _listOrdersHandler.HandleAsync(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Obtém um pedido por ID
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes do pedido</returns>
    /// <response code="200">Pedido encontrado</response>
    /// <response code="404">Pedido não encontrado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var result = await _getOrderHandler.HandleAsync(query, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Adiciona observações a um pedido
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <param name="request">Observações a serem adicionadas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Pedido atualizado</returns>
    /// <response code="200">Observações adicionadas com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Pedido não encontrado</response>
    [HttpPost("{id:guid}/notes")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderResponse>> AddNotes(
        Guid id,
        [FromBody] AddOrderNotesRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = "Observações não podem estar vazias"
            });
        }

        _logger.LogInformation("Adicionando observações ao pedido {OrderId}", id);

        var command = new AddOrderNotesCommand(id, request.Notes);
        var result = await _addNotesHandler.HandleAsync(command, cancellationToken);
        
        _logger.LogInformation("Observações adicionadas ao pedido {OrderId}", id);
        
        return Ok(result);
    }
}