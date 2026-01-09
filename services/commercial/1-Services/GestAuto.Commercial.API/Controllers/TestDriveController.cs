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
/// Gerencia test-drives de veículos
/// </summary>
[ApiController]
[Route("api/v1/test-drives")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class TestDriveController : ControllerBase
{
    private readonly ICommandHandler<ScheduleTestDriveCommand, TestDriveResponse> _scheduleHandler;
    private readonly ICommandHandler<CompleteTestDriveCommand, TestDriveResponse> _completeHandler;
    private readonly ICommandHandler<CancelTestDriveCommand, TestDriveResponse> _cancelHandler;
    private readonly IQueryHandler<GetTestDriveQuery, TestDriveResponse> _getHandler;
    private readonly IQueryHandler<ListTestDrivesQuery, PagedResponse<TestDriveListItemResponse>> _listHandler;
    private readonly ISalesPersonFilterService _salesPersonFilter;
    private readonly ILogger<TestDriveController> _logger;

    public TestDriveController(
        ICommandHandler<ScheduleTestDriveCommand, TestDriveResponse> scheduleHandler,
        ICommandHandler<CompleteTestDriveCommand, TestDriveResponse> completeHandler,
        ICommandHandler<CancelTestDriveCommand, TestDriveResponse> cancelHandler,
        IQueryHandler<GetTestDriveQuery, TestDriveResponse> getHandler,
        IQueryHandler<ListTestDrivesQuery, PagedResponse<TestDriveListItemResponse>> listHandler,
        ISalesPersonFilterService salesPersonFilter,
        ILogger<TestDriveController> logger)
    {
        _scheduleHandler = scheduleHandler;
        _completeHandler = completeHandler;
        _cancelHandler = cancelHandler;
        _getHandler = getHandler;
        _listHandler = listHandler;
        _salesPersonFilter = salesPersonFilter;
        _logger = logger;
    }

    /// <summary>
    /// Agenda um test-drive
    /// </summary>
    /// <param name="request">Dados do agendamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Test-drive agendado</returns>
    /// <response code="201">Test-drive agendado com sucesso</response>
    /// <response code="400">Dados inválidos ou veículo indisponível</response>
    /// <response code="404">Lead não encontrado</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TestDriveResponse>> Schedule(
        [FromBody] ScheduleTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var salesPersonId = GetCurrentUserId();
        _logger.LogInformation("Agendando test-drive para lead {LeadId} no veículo {VehicleId}", request.LeadId, request.VehicleId);

        try
        {
            var command = new ScheduleTestDriveCommand(
                request.LeadId,
                request.VehicleId,
                request.ScheduledAt,
                salesPersonId,
                request.Notes
            );

            var result = await _scheduleHandler.HandleAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Lead não encontrado: {Exception}", ex.Message);
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Erro de domínio ao agendar test-drive: {Exception}", ex.Message);
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// Lista test-drives com paginação
    /// </summary>
    /// <param name="leadId">ID do lead (opcional)</param>
    /// <param name="status">Status do test-drive (opcional)</param>
    /// <param name="from">Data inicial (opcional)</param>
    /// <param name="to">Data final (opcional)</param>
    /// <param name="page">Número da página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de test-drives</returns>
    /// <response code="200">Lista retornada com sucesso</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TestDriveListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<TestDriveListItemResponse>>> List(
        [FromQuery] Guid? leadId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var salesPersonId = _salesPersonFilter.GetCurrentSalesPersonId();
        _logger.LogInformation("Listando test-drives para vendedor {SalesPersonId}", salesPersonId);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        try
        {
            var query = new ListTestDrivesQuery(
                salesPersonId,
                leadId,
                status,
                from,
                to,
                page,
                pageSize);

            var result = await _listHandler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar test-drives");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails { Detail = "Erro ao listar test-drives" });
        }
    }

    /// <summary>
    /// Obtém um test-drive por ID
    /// </summary>
    /// <param name="id">ID do test-drive</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do test-drive</returns>
    /// <response code="200">Test-drive retornado com sucesso</response>
    /// <response code="404">Test-drive não encontrado</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TestDriveResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Obtendo test-drive {TestDriveId}", id);

        try
        {
            var query = new GetTestDriveQuery(id);
            var result = await _getHandler.HandleAsync(query, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Test-drive não encontrado: {Exception}", ex.Message);
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// Registra a conclusão do test-drive
    /// </summary>
    /// <remarks>
    /// Inclui checklist com quilometragem, combustível e observações.
    /// </remarks>
    /// <param name="id">ID do test-drive</param>
    /// <param name="request">Dados da conclusão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Test-drive completado</returns>
    /// <response code="200">Test-drive completado com sucesso</response>
    /// <response code="404">Test-drive não encontrado</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TestDriveResponse>> Complete(
        Guid id,
        [FromBody] CompleteTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Completando test-drive {TestDriveId}", id);

        try
        {
            var command = new CompleteTestDriveCommand(
                id,
                request.Checklist,
                request.CustomerFeedback,
                userId
            );

            var result = await _completeHandler.HandleAsync(command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Test-drive não encontrado: {Exception}", ex.Message);
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro ao completar test-drive: {Exception}", ex.Message);
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Erro de validação: {Exception}", ex.Message);
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// Cancela um test-drive agendado
    /// </summary>
    /// <param name="id">ID do test-drive</param>
    /// <param name="request">Motivo do cancelamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Test-drive cancelado</returns>
    /// <response code="200">Test-drive cancelado com sucesso</response>
    /// <response code="404">Test-drive não encontrado</response>
    /// <response code="400">Operação inválida</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(TestDriveResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TestDriveResponse>> Cancel(
        Guid id,
        [FromBody] CancelTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("Cancelando test-drive {TestDriveId}", id);

        try
        {
            var command = new CancelTestDriveCommand(id, request.Reason, userId);
            var result = await _cancelHandler.HandleAsync(command, cancellationToken);

            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Test-drive não encontrado: {Exception}", ex.Message);
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro ao cancelar test-drive: {Exception}", ex.Message);
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
    }
}
