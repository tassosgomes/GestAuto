using GestAuto.Stock.API.Services;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Commands;
using GestAuto.Stock.Application.Reservations.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAuto.Stock.API.Controllers;

[ApiController]
public sealed class ReservationsController : ControllerBase
{
    private readonly ICommandHandler<CreateReservationCommand, ReservationResponse> _createReservation;
    private readonly ICommandHandler<CancelReservationCommand, ReservationResponse> _cancelReservation;
    private readonly ICommandHandler<ExtendReservationCommand, ReservationResponse> _extendReservation;

    public ReservationsController(
        ICommandHandler<CreateReservationCommand, ReservationResponse> createReservation,
        ICommandHandler<CancelReservationCommand, ReservationResponse> cancelReservation,
        ICommandHandler<ExtendReservationCommand, ReservationResponse> extendReservation)
    {
        _createReservation = createReservation;
        _cancelReservation = cancelReservation;
        _extendReservation = extendReservation;
    }

    [HttpPost("vehicles/{vehicleId:guid}/reservations")]
    [Authorize(Policy = "SalesPerson")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromRoute] Guid vehicleId,
        [FromBody] CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var created = await _createReservation.HandleAsync(new CreateReservationCommand(vehicleId, userId, request), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPost("reservations/{reservationId:guid}/cancel")]
    [Authorize(Policy = "SalesPerson")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid reservationId,
        [FromBody] CancelReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var canCancelOthers = User.HasClaim("roles", "SALES_MANAGER") || User.HasClaim("roles", "MANAGER") || User.HasClaim("roles", "ADMIN");

        var result = await _cancelReservation.HandleAsync(
            new CancelReservationCommand(reservationId, userId, canCancelOthers, request),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("reservations/{reservationId:guid}/extend")]
    [Authorize(Policy = "SalesManager")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Extend(
        [FromRoute] Guid reservationId,
        [FromBody] ExtendReservationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _extendReservation.HandleAsync(
            new ExtendReservationCommand(reservationId, userId, request),
            cancellationToken);

        return Ok(result);
    }
}
