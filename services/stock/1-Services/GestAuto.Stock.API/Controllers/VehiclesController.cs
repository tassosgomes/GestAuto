using GestAuto.Stock.API.Services;
using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.TestDrives.Commands;
using GestAuto.Stock.Application.TestDrives.Dto;
using GestAuto.Stock.Application.Vehicles.Commands;
using GestAuto.Stock.Application.Vehicles.Dto;
using GestAuto.Stock.Application.Vehicles.Queries;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestAuto.Stock.API.Controllers;

[ApiController]
[Route("vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly ICommandHandler<CreateVehicleCommand, VehicleResponse> _createVehicle;
    private readonly IQueryHandler<GetVehicleQuery, VehicleResponse> _getVehicle;
    private readonly IQueryHandler<ListVehiclesQuery, GestAuto.Stock.Application.Common.PagedResponse<VehicleListItem>> _listVehicles;
    private readonly ICommandHandler<ChangeVehicleStatusCommand, bool> _changeStatus;
    private readonly ICommandHandler<CreateCheckInCommand, CheckInResponse> _createCheckIn;
    private readonly ICommandHandler<CreateCheckOutCommand, CheckOutResponse> _createCheckOut;
    private readonly ICommandHandler<StartTestDriveCommand, StartTestDriveResponse> _startTestDrive;

    public VehiclesController(
        ICommandHandler<CreateVehicleCommand, VehicleResponse> createVehicle,
        IQueryHandler<GetVehicleQuery, VehicleResponse> getVehicle,
        IQueryHandler<ListVehiclesQuery, GestAuto.Stock.Application.Common.PagedResponse<VehicleListItem>> listVehicles,
        ICommandHandler<ChangeVehicleStatusCommand, bool> changeStatus,
        ICommandHandler<CreateCheckInCommand, CheckInResponse> createCheckIn,
        ICommandHandler<CreateCheckOutCommand, CheckOutResponse> createCheckOut,
        ICommandHandler<StartTestDriveCommand, StartTestDriveResponse> startTestDrive)
    {
        _createVehicle = createVehicle;
        _getVehicle = getVehicle;
        _listVehicles = listVehicles;
        _changeStatus = changeStatus;
        _createCheckIn = createCheckIn;
        _createCheckOut = createCheckOut;
        _startTestDrive = startTestDrive;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] VehicleCreate request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var created = await _createVehicle.HandleAsync(new CreateVehicleCommand(userId, request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await _getVehicle.HandleAsync(new GetVehicleQuery(id), cancellationToken);
        return Ok(vehicle);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery(Name = "_page")] int _page = 1,
        [FromQuery(Name = "_size")] int _size = 10,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        // Support both task-required (page/pageSize) and repo-standard (_page/_size).
        var effectivePage = page ?? _page;
        var effectiveSize = pageSize ?? _size;

        if (effectivePage < 1 || effectiveSize < 1)
        {
            throw new DomainException("Pagination parameters must be positive.");
        }

        var parsedStatus = ParseEnum<VehicleStatus>(status);
        var parsedCategory = ParseEnum<VehicleCategory>(category);

        var response = await _listVehicles.HandleAsync(
            new ListVehiclesQuery(
                Page: effectivePage,
                Size: effectiveSize,
                Status: parsedStatus,
                Category: parsedCategory,
                Query: q),
            cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeVehicleStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        await _changeStatus.HandleAsync(
            new ChangeVehicleStatusCommand(id, request.NewStatus, request.Reason, userId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/check-ins")]
    [Authorize]
    [ProducesResponseType(typeof(CheckInResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(
        [FromRoute] Guid id,
        [FromBody] CheckInCreateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var created = await _createCheckIn.HandleAsync(new CreateCheckInCommand(id, userId, request), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, created);
    }

    [HttpPost("{id:guid}/check-outs")]
    [Authorize]
    [ProducesResponseType(typeof(CheckOutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckOut(
        [FromRoute] Guid id,
        [FromBody] CheckOutCreateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        // PRD/TechSpec: baixa por sinistro/perda total exige perfil Manager/Admin.
        if (request.Reason == CheckOutReason.TotalLoss &&
            !User.HasClaim("roles", "MANAGER") &&
            !User.HasClaim("roles", "ADMIN"))
        {
            throw new ForbiddenException("Sem permissão para baixar veículo por sinistro/perda total.");
        }

        var created = await _createCheckOut.HandleAsync(new CreateCheckOutCommand(id, userId, request), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, created);
    }

    [HttpPost("{id:guid}/test-drives/start")]
    [Authorize(Policy = "SalesPerson")]
    [ProducesResponseType(typeof(StartTestDriveResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartTestDrive(
        [FromRoute] Guid id,
        [FromBody] StartTestDriveRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var created = await _startTestDrive.HandleAsync(new StartTestDriveCommand(id, userId, request), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    private static TEnum? ParseEnum<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        throw new DomainException($"Invalid value for {typeof(TEnum).Name}: {value}");
    }
}
