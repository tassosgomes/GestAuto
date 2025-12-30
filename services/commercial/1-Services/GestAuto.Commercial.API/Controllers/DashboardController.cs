using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.API.Services;

namespace GestAuto.Commercial.API.Controllers;

/// <summary>
/// Dashboard comercial com KPIs e ações pendentes
/// </summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Policy = "SalesPerson")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IQueryHandler<GetDashboardDataQuery, DashboardResponse> _getDashboardHandler;
    private readonly ISalesPersonFilterService _salesPersonFilter;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IQueryHandler<GetDashboardDataQuery, DashboardResponse> getDashboardHandler,
        ISalesPersonFilterService salesPersonFilter,
        ILogger<DashboardController> logger)
    {
        _getDashboardHandler = getDashboardHandler;
        _salesPersonFilter = salesPersonFilter;
        _logger = logger;
    }

    /// <summary>
    /// Obtém dados do dashboard (KPIs e ações pendentes)
    /// </summary>
    /// <returns>Dados do dashboard</returns>
    /// <response code="200">Dados retornados com sucesso</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> GetDashboardData(CancellationToken cancellationToken)
    {
        try
        {
            var salesPersonGuid = _salesPersonFilter.GetCurrentSalesPersonId();
            var salesPersonId = salesPersonGuid?.ToString();

            var query = new GetDashboardDataQuery
            {
                SalesPersonId = salesPersonId
            };

            var data = await _getDashboardHandler.HandleAsync(query, cancellationToken);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar dados do dashboard");
            return StatusCode(500, "Erro ao buscar dados do dashboard");
        }
    }
}
