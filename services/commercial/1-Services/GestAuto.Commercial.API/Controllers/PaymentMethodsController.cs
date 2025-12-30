using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Infra;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestAuto.Commercial.API.Controllers;

/// <summary>
/// Controller para gerenciar formas de pagamento
/// </summary>
[ApiController]
[Route("api/v1/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly CommercialDbContext _context;
    private readonly ILogger<PaymentMethodsController> _logger;

    public PaymentMethodsController(
        CommercialDbContext context,
        ILogger<PaymentMethodsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as formas de pagamento ativas
    /// </summary>
    /// <returns>Lista de formas de pagamento ordenada por DisplayOrder</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentMethodResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentMethodResponse>>> GetPaymentMethods()
    {
        _logger.LogInformation("Buscando formas de pagamento ativas");

        var paymentMethods = await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .OrderBy(pm => pm.DisplayOrder)
            .Select(pm => PaymentMethodResponse.FromEntity(pm))
            .ToListAsync();

        return Ok(paymentMethods);
    }

    /// <summary>
    /// Obtém uma forma de pagamento específica por código
    /// </summary>
    /// <param name="code">Código da forma de pagamento (ex: CASH, FINANCING)</param>
    /// <returns>Forma de pagamento encontrada</returns>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentMethodResponse>> GetPaymentMethodByCode(string code)
    {
        _logger.LogInformation("Buscando forma de pagamento com código {Code}", code);

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.Code == code && pm.IsActive);

        if (paymentMethod == null)
        {
            _logger.LogWarning("Forma de pagamento com código {Code} não encontrada", code);
            return NotFound(new { message = $"Forma de pagamento '{code}' não encontrada" });
        }

        return Ok(PaymentMethodResponse.FromEntity(paymentMethod));
    }
}
